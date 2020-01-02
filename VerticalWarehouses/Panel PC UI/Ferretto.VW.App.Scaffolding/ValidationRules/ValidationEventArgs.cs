using System;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class ValidationEventArgs : EventArgs {
        internal ValidationEventArgs(bool isValid) : base()
        {
            this.IsValid = isValid;
        }

        public bool IsValid { get; }
    }
}
