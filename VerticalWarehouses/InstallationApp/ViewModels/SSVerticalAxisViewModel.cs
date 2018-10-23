using System.Windows.Media;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewModels
{
    internal class SSVerticalAxisViewModel : BindableBase
    {
        #region Fields

        private readonly SolidColorBrush FERRETTOGRAY = (SolidColorBrush)new BrushConverter().ConvertFrom("#707173");
        private readonly SolidColorBrush FERRETTOGREEN = (SolidColorBrush)new BrushConverter().ConvertFrom("#57A639");
        private SolidColorBrush brakeResistanceOverTemperatureFillColor;
        private SolidColorBrush emergencyEndRunFillColor;
        private SolidColorBrush verticalZeroSensorFillColor;

        #endregion Fields

        #region Constructors

        public SSVerticalAxisViewModel()
        {
            this.VerticalZeroSensorFillColor = this.FERRETTOGRAY;
            this.BrakeResistanceOverTemperatureFillColor = this.FERRETTOGRAY;
            this.EmergencyEndRunFillColor = this.FERRETTOGRAY;
        }

        #endregion Constructors

        #region Properties

        public SolidColorBrush BrakeResistanceOverTemperatureFillColor { get => this.brakeResistanceOverTemperatureFillColor; set => this.SetProperty(ref this.brakeResistanceOverTemperatureFillColor, value); }
        public SolidColorBrush EmergencyEndRunFillColor { get => this.emergencyEndRunFillColor; set => this.SetProperty(ref this.emergencyEndRunFillColor, value); }
        public SolidColorBrush VerticalZeroSensorFillColor { get => this.verticalZeroSensorFillColor; set => this.SetProperty(ref this.verticalZeroSensorFillColor, value); }

        #endregion Properties
    }
}
