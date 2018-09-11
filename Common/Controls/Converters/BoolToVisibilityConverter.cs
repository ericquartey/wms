using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
  public class BoolToVisibilityConverter : DependencyObject, IValueConverter
  {
    #region Fields

    public static readonly DependencyProperty HideProperty = DependencyProperty.Register(
        nameof(Hide),
        typeof(bool),
        typeof(BoolToVisibilityConverter));

    public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
        nameof(Invert),
        typeof(bool),
        typeof(BoolToVisibilityConverter));

    #endregion Fields

    #region Properties

    public bool Hide
    {
      get => (bool)this.GetValue(HideProperty);
      set => this.SetValue(HideProperty, value);
    }

    public bool Invert
    {
      get => (bool)this.GetValue(InvertProperty);
      set => this.SetValue(InvertProperty, value);
    }

    #endregion Properties

    #region Methods

    public Object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
      {
        throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
      }

      var visible = System.Convert.ToBoolean(value, culture);

      if ((visible && !this.Invert) || (!visible && this.Invert))
      {
        return Visibility.Visible;
      }

      return this.Hide ? Visibility.Hidden : Visibility.Collapsed;
    }

    public Object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
    {
      if (value is Visibility == false)
      {
        throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
      }

      var visible = ((Visibility)value == Visibility.Visible ? true : false);

      if ((visible && !this.Invert) || (!visible && this.Invert))
      {
        return true;
      }

      return false;
    }

    #endregion Methods
  }
}
