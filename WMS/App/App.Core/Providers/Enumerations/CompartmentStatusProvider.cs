using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class CompartmentStatusProvider : ICompartmentStatusProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentStatusesDataService compartmentStatusesDataService;

        #endregion

        #region Constructors

        public CompartmentStatusProvider(WMS.Data.WebAPI.Contracts.ICompartmentStatusesDataService compartmentStatusesDataService)
        {
            this.compartmentStatusesDataService = compartmentStatusesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.compartmentStatusesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.compartmentStatusesDataService.GetAllCountAsync();
        }

        #endregion
    }
}
