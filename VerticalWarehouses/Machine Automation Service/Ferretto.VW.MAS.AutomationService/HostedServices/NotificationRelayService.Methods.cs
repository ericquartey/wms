using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService
    {
        #region Methods

        private async Task CalibrateAxisMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.CalibrateAxisNotify(message);
        }

        private async Task CurrentPositionMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.CurrentPositionChanged(message);
        }

        private async Task ElevatorWeightCheckMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.ElevatorWeightCheck(message);

            this.Logger.LogTrace($"30:Sent SignalR Message:{receivedMessage.Type}, with Status:{receivedMessage.Status}");
        }

        private async Task HomingMethod(NotificationMessage receivedMessage, IServiceProvider serviceProvider)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.HomingProcedureStatusChanged(message);
        }

        private async Task MachineStateActiveMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.MachineStateActiveNotify(message);
        }

        private async Task MachineStatusActiveMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.MachineStatusActiveNotify(message);
        }

        private async Task OnAssignedMissionOperationChanged(AssignedMissionChangedMessageData e)
        {
            Contract.Requires(e != null);

            await this.operatorHub.Clients.All.AssignedMissionChanged(
                e.BayNumber,
                e.MissionId);
        }

        private async Task OnBayChainPositionChanged(BayChainPositionMessageData data)
        {
            Contract.Requires(data != null);

            await this.installationHub.Clients.All.BayChainPositionChanged(data.Position, data.BayNumber);
        }

        private async Task OnBayConnected(BayOperationalStatusChangedMessageData data)
        {
            Contract.Requires(data != null);

            await this.operatorHub.Clients.All.BayStatusChanged(data.BayNumber, data.BayStatus);
        }

        private async Task OnBayLight(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Status == MessageStatus.OperationEnd
                && this.machineVolatileDataProvider.IsBayLightOn.ContainsKey(receivedMessage.RequestingBay))
            {
                await this.installationHub.Clients.All.BayLightChanged(this.machineVolatileDataProvider.IsBayLightOn[receivedMessage.RequestingBay], receivedMessage.RequestingBay);
            }
        }

        private async Task OnChangeRunningState(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is ChangeRunningStateMessageData data)
            {
                MachinePowerState machinePowerState;
                switch (receivedMessage.Status)
                {
                    case MessageStatus.OperationStart:
                    case MessageStatus.OperationExecuting:
                        machinePowerState = data.Enable ? MachinePowerState.PoweringUp : MachinePowerState.PoweringDown;
                        break;

                    case MessageStatus.OperationEnd:
                        machinePowerState = data.Enable ? MachinePowerState.Powered : MachinePowerState.Unpowered;
                        this.machineVolatileDataProvider.IsHomingActive = false;
                        if (data.Enable)
                        {
                            this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(receivedMessage.RequestingBay);
                        }

                        break;

                    case MessageStatus.OperationError:
                    case MessageStatus.OperationStop:
                        machinePowerState = MachinePowerState.Unpowered;
                        this.machineVolatileDataProvider.IsHomingActive = false;
                        break;

                    default:
                        machinePowerState = data.Enable ? MachinePowerState.Unpowered : MachinePowerState.Powered;
                        break;
                }

                this.machineVolatileDataProvider.MachinePowerState = machinePowerState;

                await this.installationHub.Clients.All.MachinePowerChanged(machinePowerState);
            }
        }

        private async Task OnDataLayerReady(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("OnDataLayerReady start");
            var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
            var bays = baysDataProvider.GetAll();

            foreach (var bay in bays.Where(b => b.Carousel == null && b.External == null && b.Number != BayNumber.ElevatorBay))
            {
                this.machineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = true;
            }

            baysDataProvider.AddElevatorPseudoBay();

            var wmsSettingsProvider = serviceProvider.GetRequiredService<IWmsSettingsProvider>();
            if (wmsSettingsProvider.IsEnabled)
            {
                //var dataHubClient = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IDataHubClient>();
                wmsSettingsProvider.IsConnected = false;
                //dataHubClient.ConnectAsync(new Uri(wmsSettingsProvider.ServiceUrl, "hubs/data"));
            }

            this.Logger.LogTrace("OnDataLayerReady end");
        }

        private async Task OnElevatorPositionChanged(ElevatorPositionMessageData data)
        {
            Contract.Requires(data != null);

            await this.installationHub.Clients.All.ElevatorPositionChanged(
                data.VerticalPosition,
                data.HorizontalPosition,
                data.CellId,
                data.BayPositionId,
                data.BayPositionUpper);
        }

        private async Task OnErrorStatusChanged(IErrorStatusMessageData machineErrorMessageData)
        {
            Contract.Requires(machineErrorMessageData != null);

            await this.operatorHub.Clients.All.ErrorStatusChanged(machineErrorMessageData.ErrorId);
        }

        private async Task OnFsmException(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is FsmExceptionMessageData)
            {
                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                await this.installationHub.Clients.All.FsmException(messageToUi);
            }
        }

        private async Task OnInverterProgrammingChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);

            await this.installationHub.Clients.All.InverterProgrammingChanged(message);
        }

        private async Task OnInverterStatusWordChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.InverterStatusWordChanged(message);
        }

        private async Task OnMachineModeChanged(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is MachineModeMessageData data)
            {
                await this.installationHub.Clients.All.MachineModeChanged(data.MachineMode);
            }
        }

        private async Task OnMoveLoadingUnit(NotificationMessage receivedMessage)
        {
            var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.MoveLoadingUnit(messageToUi);
        }

        private async Task OnMoveTest(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is MoveTestMessageData)
            {
                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                await this.installationHub.Clients.All.MoveTest(messageToUi);
            }
        }

        private async Task OnPositioningChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);

            await this.installationHub.Clients.All.PositioningNotify(message);
        }

        private async Task OnProfileCalibration(NotificationMessage receivedMessage)
        {
            if (receivedMessage.Data is ProfileCalibrationMessageData)
            {
                var messageToUi = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
                await this.installationHub.Clients.All.ProfileCalibration(messageToUi);
            }
        }

        private async Task OnRepetitiveHorizontalMovementsChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.RepetitiveHorizontalMovementsNotify(message);
        }

        private async Task OnSensorsChanged(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.SensorsChanged(message);
        }

        private async Task OnSocketLinkAlphaNumericBarChange(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.SocketLinkAlphaNumericBarChange(message);
        }

        private async Task OnSocketLinkLaserPointerChange(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.SocketLinkLaserPointerChange(message);
        }

        //private async Task OnSocketLinkEnableChanged(IServiceProvider serviceProvider)
        //{
        //    var dataHubClient = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IDataHubClient>();
        //    var wmsSettingsProvider = serviceProvider.GetRequiredService<IWmsSettingsProvider>();
        //    //if (wmsSettingsProvider.SocketLinkIsEnabled)
        //    //{
        //    //    wmsSettingsProvider.IsConnected = false;
        //    //    await dataHubClient.ConnectAsync(new Uri(wmsSettingsProvider.ServiceUrl, "hubs/data"));
        //    //}
        //    //else
        //    //{
        //    //    try
        //    //    {
        //    //        if (dataHubClient.IsConnected)
        //    //        {
        //    //            await dataHubClient.DisconnectAsync();
        //    //        }
        //    //    }
        //    //    catch
        //    //    {
        //    //        // do nothing
        //    //    }
        //    //}
        //}

        private async Task OnSystemTimeChangedAsync()
        {
            await this.installationHub.Clients.All.SystemTimeChanged();
        }

        private async Task OnWmsEnableChanged(IServiceProvider serviceProvider)
        {
            var dataHubClient = serviceProvider.GetRequiredService<WMS.Data.WebAPI.Contracts.IDataHubClient>();
            var wmsSettingsProvider = serviceProvider.GetRequiredService<IWmsSettingsProvider>();
            if (wmsSettingsProvider.IsEnabled)
            {
                wmsSettingsProvider.IsConnected = false;
                await dataHubClient.ConnectAsync(new Uri(wmsSettingsProvider.ServiceUrl, "hubs/data"));
            }
            else
            {
                try
                {
                    if (dataHubClient.IsConnected)
                    {
                        await dataHubClient.DisconnectAsync();
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private async Task ResolutionCalibrationMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.ResolutionCalibrationNotify(message);
        }

        private async Task ShutterPositioningMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.ShutterPositioningNotify(message);
        }

        private async Task SwitchAxisMethod(NotificationMessage receivedMessage)
        {
            var message = NotificationMessageUiFactory.FromNotificationMessage(receivedMessage);
            await this.installationHub.Clients.All.SwitchAxisNotify(message);
        }

        #endregion
    }
}
