namespace Ferretto.VW.MAS.DataLayer.Providers.Models
{
    public class BaySetupStatus
    {
        #region Properties

        public SetupStepStatus AllLoadingUnits { get; set; }

        public SetupStepStatus Check { get; set; }

        public SetupStepStatus FirstLoadingUnit { get; set; }

        public SetupStepStatus Laser { get; set; }

        public SetupStepStatus Shape { get; set; }

        public SetupStepStatus Shutter { get; set; }

        #endregion
    }
}
