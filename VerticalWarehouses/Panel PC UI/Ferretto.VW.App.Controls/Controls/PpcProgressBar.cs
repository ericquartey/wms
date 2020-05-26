using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Controls
{
    public class PpcProgressBar : ProgressBar
    {
        #region Fields

        public static readonly DependencyProperty Color1Property = DependencyProperty.Register(
            nameof(Color1),
            typeof(SolidColorBrush),
            typeof(PpcProgressBar));

        public static readonly DependencyProperty Color2Property = DependencyProperty.Register(
            nameof(Color2),
            typeof(SolidColorBrush),
            typeof(PpcProgressBar));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            nameof(Radius),
            typeof(double),
            typeof(PpcProgressBar),
            new PropertyMetadata(2D));

        #endregion

        #region Constructors

        public PpcProgressBar()
        {
        }

        #endregion

        #region Properties

        public SolidColorBrush Color1
        {
            get => (SolidColorBrush)this.GetValue(Color1Property);
            set => this.SetValue(Color1Property, value);
        }

        public SolidColorBrush Color2
        {
            get => (SolidColorBrush)this.GetValue(Color2Property);
            set => this.SetValue(Color2Property, value);
        }

        public double Radius
        {
            get => (double)this.GetValue(RadiusProperty);
            set => this.SetValue(RadiusProperty, value);
        }

        #endregion
    }
}
