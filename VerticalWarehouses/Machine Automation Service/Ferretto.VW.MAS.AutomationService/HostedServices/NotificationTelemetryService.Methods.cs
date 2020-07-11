using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService
    {
        #region Methods

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

            var errorLog = new ErrorLog
            {
                ErrorId = error.Id,
                AdditionalText = error.AdditionalText,
                BayNumber = (int)error.BayNumber,
                Code = error.Code,
                DetailCode = error.DetailCode,
                InverterIndex = error.InverterIndex,
                OccurrenceDate = error.OccurrenceDate,
                ResolutionDate = error.ResolutionDate,
            };

            await this.SendErrorLogAsync(errorLog);
        }

        private async Task OnMoveLoadingUnitAsync(MoveLoadingUnitMessageData message)
        {
            var missionLog = new MissionLog
            {
                // TODO: fill all missing fields
                Destination = message.Destination.ToString(),
                CellId = message.DestinationCellId,
                LoadUnitId = message.LoadUnitId.Value,
                MissionId = message.MissionId.Value,
                MissionType = message.MissionType.ToString(),
                Stage = message.MissionStep.ToString(),
                StopReason = (int)message.StopReason,
                TimeStamp = DateTimeOffset.Now,
                WmsId = message.WmsId
            };

            await this.SendMissionLogAsync(missionLog);
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

        #endregion
    }
}
