using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVertimagConfigurationDataLayer
    {
        #region Methods

        Dictionary<InverterIndex, InverterType> GetInstalledInverterList();

        List<IoIndex> GetInstalledIoList();

        #endregion
    }
}
