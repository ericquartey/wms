using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class AbcClassProvider : BaseProvider, IAbcClassProvider
    {
        #region Constructors

        public AbcClassProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<AbcClass>> GetAllAsync()
        {
            return await this.DataContext.AbcClasses
               .Select(a => new AbcClass
               {
                   Id = a.Id,
                   Description = a.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.AbcClasses.CountAsync();
        }

        public async Task<AbcClass> GetByIdAsync(string id)
        {
            return await this.DataContext.AbcClasses
                 .Select(a => new AbcClass
                 {
                     Id = a.Id,
                     Description = a.Description
                 })
                 .SingleOrDefaultAsync(a => a.Id == id);
        }

        #endregion
    }
}
