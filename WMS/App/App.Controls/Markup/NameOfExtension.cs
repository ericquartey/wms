using System;
using System.Windows.Markup;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class NameOfExtension : MarkupExtension
    {
        #region Properties

        public Type Type { get; set; }

        #endregion

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Type == null)
            {
                throw new ArgumentException(Errors.MarkupNameOfSyntax);
            }

            return this.Type.Name;
        }

        #endregion
    }
}
