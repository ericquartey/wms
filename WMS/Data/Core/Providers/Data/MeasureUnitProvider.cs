using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MeasureUnitProvider : BaseProvider, IMeasureUnitProvider
    {
        #region Constructors

        public MeasureUnitProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<MeasureUnit>> GetAllAsync()
        {
            return await this.DataContext.MeasureUnits
               .Select(c => new MeasureUnit
               {
                   Id = c.Id,
                   Description = c.Description
               })
               .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await this.DataContext.MeasureUnits.CountAsync();
        }

        public async Task<MeasureUnit> GetByIdAsync(string id)
        {
            return await this.DataContext.MeasureUnits
                 .Select(c => new MeasureUnit
                 {
                     Id = c.Id,
                     Description = c.Description
                 })
                 .SingleOrDefaultAsync(m => m.Id == id);
        }

        #endregion
    }
}
