using System;

namespace Ferretto.VW.App.Scaffolding.ValidationRules
{
    public class ValidationEventArgs : EventArgs
    {
        internal ValidationEventArgs(bool isValid, object errorContent = null) : base()
        {
            this.IsValid = isValid;
            this.ErrorContent = errorContent;
        }

        public bool IsValid { get; }
        public object ErrorContent { get; }
    }
}
