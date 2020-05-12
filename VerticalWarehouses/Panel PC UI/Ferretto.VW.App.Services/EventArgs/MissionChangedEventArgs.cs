using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class MissionChangedEventArgs
    {
        #region Constructors

        public MissionChangedEventArgs(Mission machineMission, MissionWithLoadingUnitDetails wmsMission, MissionOperation wmsOperation)
        {
            if (wmsMission != null && machineMission is null)
            {
                throw new System.ArgumentException(Resources.EventArgsMissionChanged.WmsMissionWithoutMachineMission);
            }

            if (wmsMission is null && wmsOperation != null)
            {
                throw new System.ArgumentException(Resources.EventArgsMissionChanged.WmsOperationWithoutWmsMission);
            }

            this.MachineMission = machineMission;
            this.WmsMission = wmsMission;
            this.WmsOperation = wmsOperation;
        }

        #endregion

        #region Properties

        public Mission MachineMission { get; }

        public MissionWithLoadingUnitDetails WmsMission { get; }

        public MissionOperation WmsOperation { get; }

        #endregion
    }
}
