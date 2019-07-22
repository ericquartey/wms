using System.Collections.Generic;
using System.Threading.Tasks;

using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVertimagConfigurationDataLayer
    {
        #region Methods

        Task<Dictionary<InverterIndex, InverterType>> GetInstalledInverterListAsync();

        Task<List<IoIndex>> GetInstalledIoListAsync();

        #endregion
    }
}
