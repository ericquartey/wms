using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public SchedulerRequestProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<SchedulerRequest>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            IExpression whereExpression = null,
            Expression<Func<SchedulerRequest, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ToArrayAsync(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression);
        }

        public async Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            Expression<Func<SchedulerRequest, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .CountAsync(whereExpression, searchExpression);
        }

        public async Task<SchedulerRequest> GetByIdAsync(int id)
        {
            var result = await this.GetAllBase()
                             .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.SchedulerRequests,
                       this.GetAllBase());
        }

        private IQueryable<SchedulerRequest> GetAllBase()
        {
            return this.dataContext.SchedulerRequests
                .Include(r => r.Bay)
                .Include(r => r.Area)
                .Include(r => r.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(r => r.List)
                .Include(r => r.ListRow)
                .Include(r => r.LoadingUnit)
                .Include(r => r.LoadingUnitType)
                .Include(m => m.MaterialStatus)
                .Include(m => m.PackageType)
                .Select(r => new SchedulerRequest
                {
                    AreaDescription = r.Area.Name,
                    BayDescription = r.Bay.Description,
                    CreationDate = r.CreationDate,
                    DispatchedQuantity = r.DispatchedQuantity,
                    IsInstant = r.IsInstant,
                    ItemDescription = r.Item.Description,
                    ItemUnitMeasure = r.Item.MeasureUnit.Description,
                    LastModificationDate = r.LastModificationDate,
                    ListDescription = r.List.Description,
                    ListRowDescription = r.ListRow.Code,
                    LoadingUnitDescription = r.LoadingUnit.Code,
                    LoadingUnitTypeDescription = r.LoadingUnitType.Description,
                    Lot = r.Lot,
                    MaterialStatusDescription = r.MaterialStatus.Description,
                    OperationType = (OperationType)r.OperationType,
                    PackageTypeDescription = r.PackageType.Description,
                    RegistrationNumber = r.RegistrationNumber,
                    RequestedQuantity = r.RequestedQuantity,
                    Sub1 = r.Sub1,
                    Sub2 = r.Sub2,
                });
        }

        #endregion
    }
}
