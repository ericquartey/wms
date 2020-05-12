using System;
using System.Windows.Markup;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Controls
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
                throw new ArgumentException(General.MarkupNameOfSyntax);
            }

            return this.Type.Name;
        }

        #endregion
    }
}
