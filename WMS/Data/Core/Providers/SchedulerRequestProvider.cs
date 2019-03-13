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
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync<SchedulerRequest, Common.DataModels.SchedulerRequest>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<SchedulerRequest, Common.DataModels.SchedulerRequest>(
                    whereString,
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

                i.BayDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListRowDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.OperationType.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.RequestedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ;
        }

        private IQueryable<SchedulerRequest> GetAllBase()
        {
            return this.dataContext.SchedulerRequests
                .Select(r => new SchedulerRequest
                {
                    Id = r.Id,
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
