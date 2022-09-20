using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Converters
{
    public class CardSensorToBackgroundConverter : DependencyObject, IMultiValueConverter
    {
        #region Fields

        public static readonly DependencyProperty PccBackgroundProperty = DependencyProperty.Register(
            nameof(PpcBackground),
            typeof(Brush),
            typeof(CardSensorToBackgroundConverter));

        public static readonly DependencyProperty SecondColorProperty = DependencyProperty.Register(
            nameof(SecondColor),
            typeof(Brush),
            typeof(CardSensorToBackgroundConverter));

        #endregion

        #region Properties

        public Brush PpcBackground
        {
            get => (Brush)this.GetValue(PccBackgroundProperty);
            set => this.SetValue(PccBackgroundProperty, value);
        }

        public Brush SecondColor
        {
            get => (Brush)this.GetValue(SecondColorProperty);
            set => this.SetValue(SecondColorProperty, value);
        }

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var zero = (bool)values[0];
                var ant = (bool)values[1];
                var post = (bool)values[2];
                var byPass = (bool)values[3];

                if (byPass && ((!zero && !ant && !post) || ant != post))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
