using System.Collections.Generic;

namespace System
{
    public static class EnumerableExtensions
    {
        #region Methods

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            if (enumeration != null)
            {
                foreach (var item in enumeration)
                {
                    action(item);
                }
            }
        }

        #endregion
    }
}
