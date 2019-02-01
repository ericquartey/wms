using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class AbcClassProvider : IAbcClassProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public AbcClassProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<AbcClass>> GetAllAsync()
        {
            return await this.dataContext.AbcClasses
               .Select(a => new AbcClass
               {
                   Id = a.Id,
                   Description = a.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.dataContext.AbcClasses.CountAsync();
        }

        public async Task<AbcClass> GetByIdAsync(string id)
        {
            return await this.dataContext.AbcClasses
                 .Select(a => new AbcClass
                 {
                     Id = a.Id,
                     Description = a.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion Methods
    }
}
