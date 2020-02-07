using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class TextBoxValidationBehavior : TogglableBehavior<TextBox>
    {
        #region Fields

        private ValidationRule extraRule;

        #endregion

        #region Methods

        public abstract bool Validate(string text/*, CultureInfo cultureInfo*/);

        protected override void OnAttached()
        {
            base.OnAttached();
            var binding = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
            if (binding != null)
            {
                binding.ValidationRules.Add(this.extraRule = new TextBoxValidationBehaviorRule(this));
            }
        }

        protected override void OnDetaching()
        {
            if (this.extraRule != null)
            {
                var rules = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.ParentBinding?.ValidationRules;
                int ndx;
                if (rules?.Any() == true && (ndx = rules.IndexOf(this.extraRule)) >= 0)
                {
                    rules.RemoveAt(ndx);
                }
            }
            base.OnDetaching();
        }

        #endregion

        // protected bool Validate(string text) => this.Validate(text, this.AssociatedObject?.Language?.GetEquivalentCulture() ?? System.Threading.Thread.CurrentThread.CurrentCulture);

        #region Classes

        private class TextBoxValidationBehaviorRule : ValidationRule
        {
            #region Fields

            private readonly TextBoxValidationBehavior _behavior;

            #endregion

            #region Constructors

            public TextBoxValidationBehaviorRule(TextBoxValidationBehavior behavior)
            {
                this._behavior = behavior;
            }

            #endregion

            #region Methods

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                if (this._behavior.Validate(System.Convert.ToString(value, cultureInfo)))
                {
                    return ValidationResult.ValidResult;
                }
                return new ValidationResult(false, Resources.UI.GenericValidationError);
            }

            #endregion
        }

        #endregion
    }
}
