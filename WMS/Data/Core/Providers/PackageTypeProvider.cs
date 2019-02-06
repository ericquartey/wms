using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class PackageTypeProvider : IPackageTypeProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public PackageTypeProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<PackageType>> GetAllAsync()
        {
            return await this.dataContext.PackageTypes
               .Select(c => new PackageType
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.PackageTypes.CountAsync();
        }

        public async Task<PackageType> GetByIdAsync(int id)
        {
            return await this.dataContext.PackageTypes
                 .Select(c => new PackageType
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion Methods
    }
}
