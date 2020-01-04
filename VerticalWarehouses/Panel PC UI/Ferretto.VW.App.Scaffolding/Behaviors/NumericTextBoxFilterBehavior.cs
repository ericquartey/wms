using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public class NumericTextBoxFilterBehavior : TextBoxFilterBehavior
    {
        public static readonly DependencyProperty MinProperty
            = DependencyProperty.RegisterAttached("Min", typeof(double?), typeof(NumericTextBoxFilterBehavior));

        public static readonly DependencyProperty MaxProperty
                    = DependencyProperty.RegisterAttached("Max", typeof(double?), typeof(NumericTextBoxFilterBehavior));

        public static readonly DependencyProperty AllowDecimalsProperty
                    = DependencyProperty.RegisterAttached("AllowDecimals", typeof(bool), typeof(NumericTextBoxFilterBehavior), new PropertyMetadata(false));

        public double? Min
        {
            get => (double?)this.GetValue(MinProperty);
            set => this.SetValue(MinProperty, value);
        }

        public double? Max
        {
            get => (double?)this.GetValue(MaxProperty);
            set => this.SetValue(MaxProperty, value);
        }

        public bool AllowDecimals
        {
            get => (bool)this.GetValue(AllowDecimalsProperty);
            set => this.SetValue(AllowDecimalsProperty, value);
        }

        public override bool Validate(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowLeadingSign;
                if (this.AllowDecimals)
                {
                    styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                }

                if (!double.TryParse(text, styles, System.Globalization.CultureInfo.CurrentCulture, out var value))
                {
                    return false;
                }

                if (this.Min.HasValue && value < this.Min.Value)
                {
                    return false;
                }

                if (this.Max.HasValue && value > this.Max.Value)
                {
                    return false;
                }

                if (!this.AllowDecimals && value % 1D != 0D)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
