using System.Collections.Generic;
using System.Linq;

namespace Ferretto.Common.Utils.Expressions
{
    public static class ExpressionExtensions
    {
        #region Methods

        public static string ToQueryString(this IEnumerable<SortOption> sortOptions)
        {
            return sortOptions == null ?
                string.Empty :
                string.Join(",", sortOptions.Select(s => $"{s.PropertyName} {s.Direction}"));
        }

        #endregion
    }
}
