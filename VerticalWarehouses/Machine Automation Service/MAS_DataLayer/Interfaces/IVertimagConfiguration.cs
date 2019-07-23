using System.Collections.Generic;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVertimagConfiguration
    {
        #region Methods

        Dictionary<InverterIndex, InverterType> GetInstalledInverterList();

        List<IoIndex> GetInstalledIoList();

        #endregion
    }
}
