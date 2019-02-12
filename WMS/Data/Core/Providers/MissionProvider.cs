using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public MissionProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<Mission, bool>> whereExpression = null,
            Expression<Func<Mission, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression)
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(
            Expression<Func<Mission, bool>> whereExpression = null,
            Expression<Func<Mission, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(whereExpression, searchExpression)
                       .CountAsync();
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                       .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(propertyName, this.dataContext.Missions);
        }

        private IQueryable<Mission> GetAllBase()
        {
            return this.dataContext.Missions
                .Select(m => new Mission
                {
                    Id = m.Id,
                    BayId = m.BayId,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    Lot = m.Lot,
                    ItemId = m.ItemId,
                    ItemListId = m.ItemListId,
                    ItemListRowId = m.ItemListRowId,
                    LoadingUnitId = m.LoadingUnitId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,
                    Quantity = m.RequiredQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type
                });
        }

        #endregion
    }
}
