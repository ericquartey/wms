﻿using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface IInstallationHub
    {
        #region Methods

        Task BayChainPositionChanged(double position, BayNumber bayNumber);

        Task CalibrateAxisNotify(IBaseNotificationMessageUI message);

        Task CurrentPositionChanged(IBaseNotificationMessageUI message);

        Task ElevatorPositionChanged(double verticalPosition, double horizontalPosition, int? cellId, int? bayPositionId, bool? bayPositionUpper);

        Task ElevatorWeightCheck(IBaseNotificationMessageUI message);

        Task FsmException(IBaseNotificationMessageUI message);

        Task HomingProcedureStatusChanged(IBaseNotificationMessageUI message);

        Task InverterStatusWordChanged(IBaseNotificationMessageUI message);

        Task MachineModeChanged(CommonUtils.Messages.MachineMode machineMode);

        Task MachinePowerChanged(CommonUtils.Messages.MachinePowerState machinePowerState);

        Task MachineStateActiveNotify(IBaseNotificationMessageUI message);

        Task MachineStatusActiveNotify(IBaseNotificationMessageUI message);

        Task MoveLoadingUnit(IBaseNotificationMessageUI message);

        Task PositioningNotify(IBaseNotificationMessageUI message);

        Task PowerEnableNotify(IBaseNotificationMessageUI message);

        Task ResolutionCalibrationNotify(IBaseNotificationMessageUI message);

        Task SensorsChanged(IBaseNotificationMessageUI message);

        Task ShutterPositioningNotify(IBaseNotificationMessageUI message);

        Task SwitchAxisNotify(IBaseNotificationMessageUI message);

        #endregion
    }
}
