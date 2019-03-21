using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class PackageTypeProvider : IPackageTypeProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IPackageTypesDataService packageTypesDataService;

        #endregion

        #region Constructors

        public PackageTypeProvider(WMS.Data.WebAPI.Contracts.IPackageTypesDataService packageTypesDataService)
        {
            this.packageTypesDataService = packageTypesDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Enumeration>> GetAllAsync()
        {
            return (await this.packageTypesDataService.GetAllAsync())
                .Select(c => new Enumeration(c.Id, c.Description));
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.packageTypesDataService.GetAllCountAsync();
        }

        public async Task<Enumeration> GetByIdAsync(int id)
        {
            var packageType = await this.packageTypesDataService.GetByIdAsync(id);
            return new Enumeration(packageType.Id, packageType.Description);
        }

        #endregion
    }
}
