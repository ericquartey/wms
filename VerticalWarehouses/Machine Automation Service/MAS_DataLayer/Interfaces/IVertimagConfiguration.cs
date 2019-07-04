using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IVertimagConfiguration
    {
        #region Methods

        Task<Dictionary<InverterIndex, InverterType>> GetInstalledInverterListAsync();

        Task<List<IoIndex>> GetInstalledIoListAsync();

        #endregion
    }
}
