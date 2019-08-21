using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcSensorControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IndicatorBorderBrushProperty = DependencyProperty.Register(
            nameof(IndicatorBorderBrush),
            typeof(Brush),
            typeof(PpcSensorControl));

        public static readonly DependencyProperty IndicatorFillBrushProperty = DependencyProperty.Register(
            nameof(IndicatorFillBrush),
            typeof(Brush),
            typeof(PpcSensorControl));

        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register(
            nameof(SensorState),
            typeof(bool),
            typeof(PpcSensorControl));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PpcSensorControl),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public PpcSensorControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public Brush IndicatorBorderBrush
        {
            get => (Brush)this.GetValue(IndicatorBorderBrushProperty);
            set => this.SetValue(IndicatorBorderBrushProperty, value);
        }

        public Brush IndicatorFillBrush
        {
            get => (Brush)this.GetValue(IndicatorFillBrushProperty);
            set => this.SetValue(IndicatorFillBrushProperty, value);
        }

        public bool SensorState
        {
            get => (bool)this.GetValue(SensorStateProperty);
            set => this.SetValue(SensorStateProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        #endregion
    }
}
