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
    internal class SchedulerRequestProvider : BaseProvider, ISchedulerRequestProvider
    {
        #region Constructors

        public SchedulerRequestProvider(DatabaseContext dataContext, INotificationService notificationService)
            : base(dataContext, notificationService)
        {
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
                       this.DataContext.SchedulerRequests,
                       this.GetAllBase());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<SchedulerRequest, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);

            return (i) =>
                (i.BayDescription != null && i.BayDescription.Contains(search))
                || (i.ItemDescription != null && i.ItemDescription.Contains(search))
                || (i.ListDescription != null && i.ListDescription.Contains(search))
                || (i.ListRowDescription != null && i.ListRowDescription.Contains(search))
                || i.OperationType.ToString().Contains(search)
                || (successConversionAsDouble
                    && Equals(i.RequestedQuantity, searchAsDouble));
        }

        private IQueryable<SchedulerRequest> GetAllBase()
        {
            return this.DataContext.SchedulerRequests
                .Select(r => new SchedulerRequest
                {
                    Id = r.Id,
                    AreaDescription = r.Area.Name,
                    BayDescription = r.Bay.Description,
                    CreationDate = r.CreationDate,
                    ReservedQuantity = r.ReservedQuantity,
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
                    Type = (SchedulerRequestType)r.Type,
                    Status = (SchedulerRequestStatus)r.Status,
                });
        }

        #endregion
    }
}
