using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class EnumerableExtensions
    {
        #region Methods

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            if (enumeration.IsNotNull())
            {
                foreach (T item in enumeration)
                {
                    action(item);
                }
            }
        }

        #endregion
    }
}
