using System.Collections.Generic;

namespace Ferretto.VW.App.Controls.Utils
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
