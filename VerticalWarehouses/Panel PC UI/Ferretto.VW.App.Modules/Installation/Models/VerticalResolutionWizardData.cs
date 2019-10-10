using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Installation.Models
{
    public class VerticalResolutionWizardData : BindableBase
    {
        #region Fields

        private decimal? currentResolution;

        private double? finalPosition;

        private double? initialPosition;

        private double? measuredInitialPosition;

        #endregion

        #region Properties

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public double? FinalPosition
        {
            get => this.finalPosition;
            set => this.SetProperty(ref this.finalPosition, value);
        }

        public double? InitialPosition
        {
            get => this.initialPosition;
            set => this.SetProperty(ref this.initialPosition, value);
        }

        public double? MeasuredInitialPosition
        {
            get => this.measuredInitialPosition;
            set => this.SetProperty(ref this.measuredInitialPosition, value);
        }

        #endregion
    }
}
