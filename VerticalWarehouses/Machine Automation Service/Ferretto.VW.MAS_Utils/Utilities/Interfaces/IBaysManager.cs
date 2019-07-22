using System.Collections.Generic;

namespace Ferretto.VW.MAS_Utils.Utilities.Interfaces
{
    public interface IBaysManager
    {
        #region Properties

        IEnumerable<Bay> Bays { get; }

        #endregion
    }
}
