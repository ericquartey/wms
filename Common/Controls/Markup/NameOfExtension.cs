using System;
using System.Windows.Markup;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class NameOfExtension : MarkupExtension
    {
        #region Properties

        public Type Type { get; set; }

        #endregion Properties

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Type == null)
            {
                throw new ArgumentException(Errors.MarkupNameOfSyntax);
            }

            return this.Type.Name;
        }

        #endregion Methods
    }
}
