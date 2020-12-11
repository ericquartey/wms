using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService
    {
        #region Methods

        private static string ConvertBoolArrayToStringOfBit(bool[] arrayOfBool)
        {
            var result = "";

            foreach (var b in arrayOfBool)
            {
                result += b ? "1" : "0";
            }

            return result.ToString();
        }

        private async Task OnDataLayerReadyAsync()
        {
            this.Logger.LogTrace("Data layer is ready: connecting to telemetry service hub ...");

            await this.telemetryHub.ConnectAsync();
        }

        private async Task OnErrorStatusChangedAsync(
            ErrorStatusMessageData messageData,
            IServiceProvider serviceProvider)
        {
            var error = serviceProvider
                .GetRequiredService<IErrorsProvider>()
                .GetById(messageData.ErrorId);

            if (error is null)
            {
                this.Logger.LogWarning(
                    "Unable to send error log to telemetry service because error {id} was not found.",
                    messageData.ErrorId);
            }

            var description = "";
            if (error.Description != null)
            {
                description = $"{error?.Description} ";
            }
            if (error?.AdditionalText != null)
            {
                description += $"{error?.AdditionalText}";
            }

            var errorLog = new ErrorLog
            {
                ErrorId = error.Id,
                AdditionalText = description,
                BayNumber = (int)error.BayNumber,
                Code = error.Code,
                DetailCode = error.DetailCode,
                InverterIndex = error.InverterIndex,
                OccurrenceDate = error.OccurrenceDate,
                ResolutionDate = error.ResolutionDate,
            };

            await this.SendErrorLogAsync(errorLog);
        }

        private async Task OnMachineModeChangedAsync(MachineModeMessageData messageData)
        {
            var errorLog = new ErrorLog
            {
                ErrorId = int.Parse(DateTime.Now.ToString("-MMddHHmmss")),
                AdditionalText = "MachineMode " + messageData.MachineMode.ToString(),
                BayNumber = 0,
                Code = 0,
                DetailCode = (int)messageData.MachineMode,
                InverterIndex = 0,
                OccurrenceDate = DateTimeOffset.Now,
                ResolutionDate = null,
            };

            await this.SendErrorLogAsync(errorLog);
        }

        private async Task OnMachineStatePowerUpStartAsync(MachineStateActiveMessageData messageData)
        {
            var errorLog = new ErrorLog
            {
                ErrorId = int.Parse(DateTime.Now.ToString("-MMddHHmmss")),
                BayNumber = 0,
                AdditionalText = "MachineState " + messageData.ToString(),
                Code = 0,
                DetailCode = (int)messageData.MessageActor,
                OccurrenceDate = DateTimeOffset.Now,
                ResolutionDate = null,
                InverterIndex = 0,
            };

            await this.SendErrorLogAsync(errorLog);
        }

        private async Task OnMoveLoadingUnitAsync(/*MoveLoadingUnitMessageData message,*/ NotificationMessage message)
        {
            var messageData = (MoveLoadingUnitMessageData)message.Data;

            var missionLog = new MissionLog
            {
                // TODO: fill all missing fields
                Bay = (int)message.RequestingBay,
                CreationDate = messageData.CreationDate,
                Destination = messageData.Destination.ToString(),
                CellId = messageData.DestinationCellId,
                LoadUnitId = messageData.LoadUnitId.Value,
                MissionId = messageData.MissionId.Value,
                MissionType = messageData.MissionType.ToString(),
                Status = messageData.MissionStep.ToString(),
                Step = (int)messageData.MissionStep,
                Stage = string.Empty,
                StopReason = (int)messageData.StopReason,
                TimeStamp = DateTimeOffset.Now,
                WmsId = messageData.WmsId
            };

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var missionProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                if (messageData.MissionId.HasValue)
                {
                    try
                    {
                        var mission = missionProvider.GetById(messageData.MissionId.Value);
                        missionLog.WmsId = mission.WmsId;
                    }
                    catch (EntityNotFoundException)
                    {
                        // do nothing
                    }
                }
            }

            // Send mission log
            await this.SendMissionLogAsync(missionLog);

            if (messageData.MissionId.HasValue)
            {
                // Only if mission is of type IN and current Step is the MissionStep.End
                if (messageData.MissionType == CommonUtils.Messages.Enumerations.MissionType.IN &&
                    messageData.MissionStep == CommonUtils.Messages.Enumerations.MissionStep.End &&
                    messageData.StopReason == CommonUtils.Messages.Enumerations.StopRequestReason.NoReason)
                {
                    // Retrieve the (raw) database content
                    var dataLayer = this.ServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDataLayerService>();
                    var rawDatabaseContent = dataLayer.GetRawDatabaseContent();

                    // Send raw database content
                    await this.SendRawDatabaseContentAsync(rawDatabaseContent);
                }
            }
        }

        private async Task OnSensorsChanged(NotificationMessage receivedMessage, SensorsChangedMessageData messageData)
        {
            if (receivedMessage.RequestingBay == CommonUtils.Messages.Enumerations.BayNumber.BayOne) // only bay one, beacause the other bay have the same massage input
            {
                var ioLog = new IOLog
                {
                    BayNumber = (int)receivedMessage.RequestingBay,
                    Description = receivedMessage.Description,
                    Input = NotificationTelemetryService.ConvertBoolArrayToStringOfBit(messageData.SensorsStates),
                    Output = null,
                    TimeStamp = DateTimeOffset.Now
                };

                await this.SendIOLogAsync(ioLog);
            }
        }

        private async Task SendErrorLogAsync(ErrorLog errorLog)
        {
            if (!this.telemetryHub.IsConnected)
            {
                this.Logger.LogWarning("Unable to send error log to telemetry service because the hub is not connected.");

                return;
            }

            try
            {
                await this.telemetryHub.SendErrorLogAsync(errorLog);
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Unable to send error log to telemetry service.");
            }
        }

        private async Task SendIOLogAsync(IOLog ioLog)
        {
            if (!this.telemetryHub.IsConnected)
            {
                this.Logger.LogWarning("Unable to send IO log to telemetry service because the hub is not connected.");

                return;
            }

            try
            {
                await this.telemetryHub.SendIOLogAsync(ioLog);
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Unable to send IO log to telemetry service.");
            }
        }

        private async Task SendMissionLogAsync(MissionLog missionLog)
        {
            if (!this.telemetryHub.IsConnected)
            {
                this.Logger.LogWarning("Unable to send mission log to telemetry service because the hub is not connected.");

                return;
            }

            try
            {
                await this.telemetryHub.SendMissionLogAsync(missionLog);
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Unable to send mission log to telemetry service.");
            }
        }

        private async Task SendRawDatabaseContentAsync(byte[] rawDatabaseContent)
        {
            if (!this.telemetryHub.IsConnected)
            {
                this.Logger.LogWarning("Unable to send raw database content to telemetry service because the hub is not connected.");

                return;
            }

            try
            {
                await this.telemetryHub.SendRawDatabaseContentAsync(rawDatabaseContent);
            }
            catch (Exception ex)
            {
                this.Logger.LogWarning(ex, "Unable to send raw database content to telemetry service.");
            }
        }

        #endregion
    }
}
