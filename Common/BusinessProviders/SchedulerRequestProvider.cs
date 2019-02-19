using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.ISchedulerRequestsDataService schedulerRequestsDataService;

        #endregion

        #region Constructors

        public SchedulerRequestProvider(
            WMS.Data.WebAPI.Contracts.ISchedulerRequestsDataService schedulerRequestsDataService)
        {
            this.schedulerRequestsDataService = schedulerRequestsDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<SchedulerRequest>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.schedulerRequestsDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
                .Select(r => new SchedulerRequest
                {
                    BayDescription = r.BayDescription,
                    ItemDescription = r.ItemDescription,
                    ListDescription = r.ListDescription,
                    ListRowDescription = r.ListRowDescription,
                    LoadingUnitDescription = r.LoadingUnitDescription,
                    IsInstant = r.IsInstant,
                    RequestedQuantity = r.RequestedQuantity,
                    OperationType = (OperationType)r.OperationType,

                    LoadingUnitTypeDescription = r.LoadingUnitTypeDescription,
                    RegistrationNumber = r.RegistrationNumber,
                    Lot = r.Lot,
                    DispatchedQuantity = r.DispatchedQuantity,
                    MaterialStatusDescription = r.MaterialStatusDescription,
                    Sub1 = r.Sub1,
                    Sub2 = r.Sub2,
                    CreationDate = r.CreationDate,
                    LastModificationDate = r.LastModificationDate,
                    PackageTypeDescription = r.PackageTypeDescription,
                    AreaDescription = r.AreaDescription,

                    ItemUnitMeasure = r.ItemUnitMeasure
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.schedulerRequestsDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
        }

        public async Task<SchedulerRequest> GetByIdAsync(int id)
        {
            var schedulerRequest = await this.schedulerRequestsDataService.GetByIdAsync(id);
            return new SchedulerRequest
            {
                BayDescription = schedulerRequest.BayDescription,
                ItemDescription = schedulerRequest.ItemDescription,
                ListDescription = schedulerRequest.ListDescription,
                ListRowDescription = schedulerRequest.ListRowDescription,
                LoadingUnitDescription = schedulerRequest.LoadingUnitDescription,
                IsInstant = schedulerRequest.IsInstant,
                RequestedQuantity = schedulerRequest.RequestedQuantity,
                OperationType = (OperationType)schedulerRequest.OperationType,

                LoadingUnitTypeDescription = schedulerRequest.LoadingUnitTypeDescription,
                RegistrationNumber = schedulerRequest.RegistrationNumber,
                Lot = schedulerRequest.Lot,
                DispatchedQuantity = schedulerRequest.DispatchedQuantity,
                MaterialStatusDescription = schedulerRequest.MaterialStatusDescription,
                Sub1 = schedulerRequest.Sub1,
                Sub2 = schedulerRequest.Sub2,
                CreationDate = schedulerRequest.CreationDate,
                LastModificationDate = schedulerRequest.LastModificationDate,
                PackageTypeDescription = schedulerRequest.PackageTypeDescription,
                AreaDescription = schedulerRequest.AreaDescription,

                ItemUnitMeasure = schedulerRequest.ItemUnitMeasure
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.schedulerRequestsDataService.GetUniqueValuesAsync(propertyName);
        }

        #endregion
    }
}
