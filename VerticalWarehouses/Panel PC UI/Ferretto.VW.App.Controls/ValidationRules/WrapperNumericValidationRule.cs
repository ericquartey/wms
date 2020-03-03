using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ferretto.VW.App.Controls.ValidationRules
{
    public class WrapperNumericValidationRule : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty MaxValueProperty =
             DependencyProperty.Register(nameof(MaxValue), typeof(double),
             typeof(WrapperNumericValidationRule), new FrameworkPropertyMetadata(double.MaxValue));

        public static readonly DependencyProperty MinValueProperty =
             DependencyProperty.Register(nameof(MinValue), typeof(double),
             typeof(WrapperNumericValidationRule), new FrameworkPropertyMetadata(double.MinValue));

        #endregion

        #region Properties

        public double MaxValue
        {
            get { return (double)this.GetValue(MaxValueProperty); }
            set { this.SetValue(MaxValueProperty, value); }
        }

        public double MinValue
        {
            get { return (double)this.GetValue(MinValueProperty); }
            set { this.SetValue(MinValueProperty, value); }
        }

        #endregion
    }
}
