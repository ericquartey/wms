using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.Utils.Expressions
{
    public static class ExpressionExtensions
    {
        #region Methods

        public static string ToQueryString(this IEnumerable<SortOption> sortOptions)
        {
            if (sortOptions == null)
            {
                return string.Empty;
            }

            return string.Join(",", sortOptions.Select(s => $"{s.PropertyName} {s.Direction}"));
        }

        #endregion
    }
}
