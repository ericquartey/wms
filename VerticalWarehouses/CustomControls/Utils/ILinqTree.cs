using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.CustomControls
{
    public interface ILinqTree<T>
    {
        #region Properties

        T Parent { get; }

        #endregion

        #region Methods

        IEnumerable<T> Children();

        #endregion
    }
}
