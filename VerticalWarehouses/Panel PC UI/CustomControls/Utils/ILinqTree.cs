using System.Collections.Generic;

namespace Ferretto.VW.CustomControls
{
    public interface ILinqTree<out T>
    {
        #region Properties

        T Parent { get; }

        #endregion

        #region Methods

        IEnumerable<T> Children();

        #endregion
    }
}
