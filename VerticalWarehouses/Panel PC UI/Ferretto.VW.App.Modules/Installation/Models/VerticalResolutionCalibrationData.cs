using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Installation.Models
{
    public class VerticalResolutionCalibrationData : BindableBase
    {
        #region Fields

        private decimal? currentResolution;

        private decimal? finalPosition;

        private decimal? initialPosition;

        private decimal? measuredInitialPosition;

        #endregion

        #region Properties

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public decimal? FinalPosition
        {
            get => this.finalPosition;
            set => this.SetProperty(ref this.finalPosition, value);
        }

        public decimal? InitialPosition
        {
            get => this.initialPosition;
            set => this.SetProperty(ref this.initialPosition, value);
        }

        public decimal? MeasuredInitialPosition
        {
            get => this.measuredInitialPosition;
            set => this.SetProperty(ref this.measuredInitialPosition, value);
        }

        #endregion
    }
}
