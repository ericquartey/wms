using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class InstallationHubClient : AutoReconnectHubClient, IInstallationHubClient
    {
        #region Constructors

        public InstallationHubClient(Uri url, string installationHubPath)
            : base(new Uri(url, installationHubPath))
        {
        }

        #endregion

        #region Events

        public event EventHandler<BayChainPositionChangedEventArgs> BayChainPositionChanged;

        public event EventHandler<BayLightChangedEventArgs> BayLightChanged;

        public event EventHandler<ElevatorPositionChangedEventArgs> ElevatorPositionChanged;

        public event EventHandler<MachineModeChangedEventArgs> MachineModeChanged;

        public event EventHandler<MachinePowerChangedEventArgs> MachinePowerChanged;

        public event EventHandler<MessageNotifiedEventArgs> MessageReceived;

        public event EventHandler<SystemTimeChangedEventArgs> SystemTimeChanged;

        #endregion

        #region Methods

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On<double, BayNumber>(
                nameof(IInstallationHub.BayChainPositionChanged), this.OnBayChainPositionChanged);

            connection.On<MachineMode>(
                nameof(IInstallationHub.MachineModeChanged), this.OnMachineModeChanged);

            connection.On<MachinePowerState>(
                nameof(IInstallationHub.MachinePowerChanged), this.OnMachinePowerChanged);

            connection.On<double, double, int?, int?, bool?>(
                nameof(IInstallationHub.ElevatorPositionChanged), this.OnElevatorPositionChanged);

            connection.On<NotificationMessageUI<SensorsChangedMessageData>>(
                nameof(IInstallationHub.SensorsChanged), this.OnSensorsChanged);

            connection.On<NotificationMessageUI<CalibrateAxisMessageData>>(
                nameof(IInstallationHub.CalibrateAxisNotify), this.OnCalibrateAxisNotify);

            connection.On<NotificationMessageUI<SwitchAxisMessageData>>(
                nameof(IInstallationHub.SwitchAxisNotify), this.OnSwitchAxisNotify);

            connection.On<NotificationMessageUI<ShutterPositioningMessageData>>(
                nameof(IInstallationHub.ShutterPositioningNotify), this.OnShutterPositioningNotify);

            connection.On<NotificationMessageUI<SocketLinkAlphaNumericBarChangeMessageData>>(
                nameof(IInstallationHub.SocketLinkAlphaNumericBarChange), this.OnSocketLinkAlphaNumericBarNotify);

            connection.On<NotificationMessageUI<SocketLinkLaserPointerChangeMessageData>>(
                nameof(IInstallationHub.SocketLinkLaserPointerChange), this.OnSocketLinkLaserPointerNotify);

            connection.On<NotificationMessageUI<PositioningMessageData>>(
                nameof(IInstallationHub.PositioningNotify), this.OnPositioningNotify);

            connection.On<NotificationMessageUI<InverterParametersMessageData>>(
               nameof(IInstallationHub.InverterParameterNotify), this.OnInverterParameterNotify);

            connection.On<NotificationMessageUI<HomingMessageData>>(
                nameof(IInstallationHub.HomingProcedureStatusChanged), this.OnHomingProcedureStatusChanged);

            connection.On<NotificationMessageUI<ResolutionCalibrationMessageData>>(
                nameof(IInstallationHub.ResolutionCalibrationNotify), this.OnResolutionCalibrationNotify);

            connection.On<NotificationMessageUI<InverterStatusWordMessageData>>(
                nameof(IInstallationHub.InverterStatusWordChanged), this.OnInverterStatusWordChanged);

            connection.On<NotificationMessageUI<MachineStatusActiveMessageData>>(
                nameof(IInstallationHub.MachineStatusActiveNotify), this.OnMachineStatusActiveNotify);

            connection.On<NotificationMessageUI<MachineStateActiveMessageData>>(
                nameof(IInstallationHub.MachineStateActiveNotify), this.OnMachineStateActiveNotify);

            connection.On<NotificationMessageUI<PowerEnableMessageData>>(
                nameof(IInstallationHub.PowerEnableNotify), this.OnPowerEnableNotify);

            connection.On<NotificationMessageUI<ElevatorWeightCheckMessageData>>(
                nameof(IInstallationHub.ElevatorWeightCheck), this.OnElavtorWeightCheck);

            connection.On<NotificationMessageUI<MoveLoadingUnitMessageData>>(
                nameof(IInstallationHub.MoveLoadingUnit), this.OnMoveLoadingUnit);

            connection.On<NotificationMessageUI<FsmExceptionMessageData>>(
                nameof(IInstallationHub.FsmException), this.OnFsmException);

            connection.On<NotificationMessageUI<ProfileCalibrationMessageData>>(
                nameof(IInstallationHub.ProfileCalibration), this.OnProfileCalibrationNotify);

            connection.On<NotificationMessageUI<MoveTestMessageData>>(
                nameof(IInstallationHub.MoveTest), this.OnMoveTestNotify);

            connection.On<bool, BayNumber>(
                nameof(IInstallationHub.BayLightChanged), this.OnBayLightChanged);

            connection.On(
                nameof(IInstallationHub.SystemTimeChanged), this.OnSystemTimeChanged);

            connection.On<NotificationMessageUI<RepetitiveHorizontalMovementsMessageData>>(
                nameof(IInstallationHub.RepetitiveHorizontalMovementsNotify), this.OnRepetitiveHorizontalMovementsNotify);

            connection.On<NotificationMessageUI<InverterProgrammingMessageData>>(
                nameof(IInstallationHub.InverterProgrammingChanged), this.OnInverterProgramming);

            connection.On<NotificationMessageUI<InverterReadingMessageData>>(
                nameof(IInstallationHub.InverterReadingChanged), this.OnInverterReading);

            connection.On<NotificationMessageUI<CombinedMovementsMessageData>>(
                nameof(IInstallationHub.CombinedMovementsNotify), this.OnCombinedMovementsNotify);
        }

        private void OnBayChainPositionChanged(double position, BayNumber bayNumber)
        {
            this.BayChainPositionChanged?.Invoke(this, new BayChainPositionChangedEventArgs(position, bayNumber));
        }

        private void OnBayLightChanged(bool isLightOn, BayNumber bayNumber)
        {
            this.BayLightChanged?.Invoke(this, new BayLightChangedEventArgs(isLightOn, bayNumber));
        }

        private void OnCalibrateAxisNotify(NotificationMessageUI<CalibrateAxisMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnCombinedMovementsNotify(NotificationMessageUI<CombinedMovementsMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnElavtorWeightCheck(NotificationMessageUI<ElevatorWeightCheckMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnElevatorPositionChanged(double verticalPosition, double horizontalPosition, int? cellId, int? bayPositionId, bool? bayPositionUpper)
        {
            this.ElevatorPositionChanged?.Invoke(
                this,
                new ElevatorPositionChangedEventArgs(verticalPosition, horizontalPosition, cellId, bayPositionId, bayPositionUpper));
        }

        private void OnFsmException(NotificationMessageUI<FsmExceptionMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnInverterParameterNotify(NotificationMessageUI<InverterParametersMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnInverterProgramming(NotificationMessageUI<InverterProgrammingMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnInverterReading(NotificationMessageUI<InverterReadingMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnInverterStatusWordChanged(NotificationMessageUI<InverterStatusWordMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMachineModeChanged(MachineMode machineMode)
        {
            this.MachineModeChanged?.Invoke(this, new MachineModeChangedEventArgs(machineMode));
        }

        private void OnMachinePowerChanged(MachinePowerState machinePowerState)
        {
            this.MachinePowerChanged?.Invoke(this, new MachinePowerChangedEventArgs(machinePowerState));
        }

        private void OnMachineStateActiveNotify(NotificationMessageUI<MachineStateActiveMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMachineStatusActiveNotify(NotificationMessageUI<MachineStatusActiveMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMoveLoadingUnit(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnMoveTestNotify(NotificationMessageUI<MoveTestMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnPositioningNotify(NotificationMessageUI<PositioningMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnPowerEnableNotify(NotificationMessageUI<PowerEnableMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnProfileCalibrationNotify(NotificationMessageUI<ProfileCalibrationMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnRepetitiveHorizontalMovementsNotify(NotificationMessageUI<RepetitiveHorizontalMovementsMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnResolutionCalibrationNotify(NotificationMessageUI<ResolutionCalibrationMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnShutterPositioningNotify(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSocketLinkAlphaNumericBarNotify(NotificationMessageUI<SocketLinkAlphaNumericBarChangeMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSocketLinkLaserPointerNotify(NotificationMessageUI<SocketLinkLaserPointerChangeMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSwitchAxisNotify(NotificationMessageUI<SwitchAxisMessageData> message)
        {
            this.MessageReceived?.Invoke(this, new MessageNotifiedEventArgs(message));
        }

        private void OnSystemTimeChanged()
        {
            this.SystemTimeChanged?.Invoke(this, new SystemTimeChangedEventArgs());
        }

        #endregion
    }
}
