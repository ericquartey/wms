namespace Ferretto.VW.MAS.DataLayer
{
    public class BaySetupStatus
    {
        #region Fields

        internal static readonly BaySetupStatus Complete = new BaySetupStatus
        {
            AllLoadingUnits = SetupStepStatus.Complete,
            Check = SetupStepStatus.Complete,
            FirstLoadingUnit = SetupStepStatus.Complete,
            Laser = SetupStepStatus.Complete,
            Profile = SetupStepStatus.Complete,
            Shutter = SetupStepStatus.Complete,
        };

        #endregion

        #region Properties

        public SetupStepStatus AllLoadingUnits { get; set; }

        public SetupStepStatus Check { get; set; }

        public SetupStepStatus FirstLoadingUnit { get; set; }

        public SetupStepStatus Laser { get; set; }

        public SetupStepStatus Profile { get; set; }

        public SetupStepStatus Shutter { get; set; }

        #endregion
    }
}
