namespace Ferretto.VW.MAS.DataLayer
{
    public class BaySetupStatus
    {
        #region Fields

        internal static readonly BaySetupStatus Complete = new BaySetupStatus
        {
            CarouselCalibration = SetupStepStatus.Complete,
            ExternalBayCalibration = SetupStepStatus.Complete,
            Check = SetupStepStatus.Complete,
            Laser = SetupStepStatus.Complete,
            Profile = SetupStepStatus.Complete,
            Shutter = SetupStepStatus.Complete,
            FullTest = SetupStepStatus.Complete,
        };

        #endregion

        #region Properties

        public SetupStepStatus CarouselCalibration { get; set; }

        public SetupStepStatus Check { get; set; }

        public SetupStepStatus ExternalBayCalibration { get; set; }

        public bool IsAllTestCompleted { get; set; }

        public SetupStepStatus Laser { get; set; }

        public SetupStepStatus Profile { get; set; }

        public SetupStepStatus Shutter { get; set; }

        public SetupStepStatus FullTest { get; set; }

        #endregion
    }
}
