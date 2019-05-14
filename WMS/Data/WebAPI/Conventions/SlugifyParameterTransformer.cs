using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace Ferretto.WMS.Data.WebAPI
{
    internal class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        #region Fields

        private static readonly Regex CapitalStringRegex = new Regex("([a-z])([A-Z])", RegexOptions.Compiled);

        #endregion

        #region Methods

        public string TransformOutbound(object value)
        {
            if (value == null)
            {
                return null;
            }

            return CapitalStringRegex.Replace(value.ToString(), "$1-$2").ToLower();
        }

        #endregion
    }
}
