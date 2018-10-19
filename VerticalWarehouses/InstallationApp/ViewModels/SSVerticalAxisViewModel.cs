using System.Windows.Media;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewModels
{
    internal class SSVerticalAxisViewModel : BindableBase
    {
        #region Fields

        private readonly SolidColorBrush FERRETTOGREEN = (SolidColorBrush)new BrushConverter().ConvertFrom("#57A639");
        private readonly SolidColorBrush FERRETTORED = (SolidColorBrush)new BrushConverter().ConvertFrom("#e2001a");

        private SolidColorBrush brakeResistanceOverTemperatureFillColor;
        private SolidColorBrush emergencyEndRunFillColor;
        private SolidColorBrush verticalZeroSensorFillColor;

        #endregion Fields

        #region Constructors

        public SSVerticalAxisViewModel()
        {
            this.VerticalZeroSensorFillColor = this.FERRETTORED;
            this.BrakeResistanceOverTemperatureFillColor = this.FERRETTORED;
            this.EmergencyEndRunFillColor = this.FERRETTORED;
        }

        #endregion Constructors

        #region Properties

        public SolidColorBrush BrakeResistanceOverTemperatureFillColor { get => this.brakeResistanceOverTemperatureFillColor; set => this.SetProperty(ref this.brakeResistanceOverTemperatureFillColor, value); }
        public SolidColorBrush EmergencyEndRunFillColor { get => this.emergencyEndRunFillColor; set => this.SetProperty(ref this.emergencyEndRunFillColor, value); }
        public SolidColorBrush VerticalZeroSensorFillColor { get => this.verticalZeroSensorFillColor; set => this.SetProperty(ref this.verticalZeroSensorFillColor, value); }

        #endregion Properties
    }
}
