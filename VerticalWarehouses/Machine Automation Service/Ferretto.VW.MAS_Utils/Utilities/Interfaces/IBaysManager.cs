using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_Utils.Utilities.Interfaces
{
    public interface IBaysManager
    {
        #region Properties

        List<Bay> Bays { get; set; }

        #endregion
    }
}
