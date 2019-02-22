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
    internal class SchedulerRequestProvider : ISchedulerRequestProvider
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
            IEnumerable<SortOption> orderBy = null,
            string whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync(
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<SchedulerRequest> GetByIdAsync(int id)
        {
            var result = await this.GetAllBase()
                             .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.SchedulerRequests,
                       this.GetAllBase());
        }

        private static Expression<Func<SchedulerRequest, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.AreaDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.BayDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemUnitMeasure.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListRowDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.LoadingUnitDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.LoadingUnitTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Lot.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.MaterialStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.PackageTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.RegistrationNumber.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Sub1.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Sub2.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
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
