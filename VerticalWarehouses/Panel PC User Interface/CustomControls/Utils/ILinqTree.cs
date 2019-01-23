using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.CustomControls
{
    /// <summary>
    /// Defines an interface that must be implemented to generate the LinqToTree methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILinqTree<T>
    {
        #region Properties

        T Parent { get; }

        #endregion Properties

        #region Methods

        IEnumerable<T> Children();

        #endregion Methods
    }
}
