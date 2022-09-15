using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ClassToForegroundConverter : DependencyObject, IValueConverter
    {
        #region Fields

        private const string ROTATION_CLASS_A = "A";

        private const string ROTATION_CLASS_B = "B";

        private const string ROTATION_CLASS_C = "C";

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var loadingUnit = value as LoadingUnit;

                if (loadingUnit is null)
                {
                    throw new ArgumentNullException();
                }

                var res = 0;
                if (loadingUnit.Cell != null)
                {
                    if (loadingUnit.RotationClass == ROTATION_CLASS_A)
                    {
                        res += 1;
                    }
                    else if (loadingUnit.RotationClass == ROTATION_CLASS_B)
                    {
                        res += 2;
                    }
                    else if (loadingUnit.RotationClass == ROTATION_CLASS_C)
                    {
                        res += 3;
                    }

                    if (loadingUnit.Cell.RotationClass == ROTATION_CLASS_A)
                    {
                        res -= 1;
                    }
                    else if (loadingUnit.Cell.RotationClass == ROTATION_CLASS_B)
                    {
                        res -= 2;
                    }
                    else if (loadingUnit.Cell.RotationClass == ROTATION_CLASS_C)
                    {
                        res -= 3;
                    }

                    switch (Math.Abs(res))
                    {
                        case 1:
                            return Brushes.Yellow;

                        case 2:
                            return Brushes.Orange;
                    }
                }

                var converter = new BrushConverter();
                var themeService = ServiceLocator.Current.GetInstance<IThemeService>();

                return themeService.ActiveTheme == App.Services.Models.ApplicationTheme.Dark ? (Brush)converter.ConvertFromString("#CCCCCC") : (Brush)converter.ConvertFromString("#3C3C3C");
            }
            catch (Exception)
            {
                var converter = new BrushConverter();
                var themeService = ServiceLocator.Current.GetInstance<IThemeService>();

                return themeService.ActiveTheme == App.Services.Models.ApplicationTheme.Dark ? (Brush)converter.ConvertFromString("#CCCCCC") : (Brush)converter.ConvertFromString("#3C3C3C");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
