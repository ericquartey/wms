using System;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public abstract class NotifyValidationRule : ValidationRule
    {
        public event EventHandler<ValidationEventArgs> Validated;

        protected virtual void OnValidated(ValidationEventArgs e)
        {
            this.Validated?.Invoke(this, e);
        }
    }
}
