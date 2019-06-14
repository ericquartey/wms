using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
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
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            try
            {
                var schedulerRequests = await this.schedulerRequestsDataService
                    .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

                return schedulerRequests
                    .Select(r => new SchedulerRequest
                    {
                        Id = r.Id,
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
                        ReservedQuantity = r.ReservedQuantity,
                        MaterialStatusDescription = r.MaterialStatusDescription,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                        CreationDate = r.CreationDate,
                        LastModificationDate = r.LastModificationDate,
                        PackageTypeDescription = r.PackageTypeDescription,
                        AreaDescription = r.AreaDescription,
                        ItemUnitMeasure = r.ItemUnitMeasure,
                        Type = (SchedulerRequestType)r.Type,
                        Status = (SchedulerRequestStatus)r.Status,
                    });
            }
            catch
            {
                return new List<SchedulerRequest>();
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.schedulerRequestsDataService.GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<SchedulerRequest> GetByIdAsync(int id)
        {
            try
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
                    ReservedQuantity = schedulerRequest.ReservedQuantity,
                    MaterialStatusDescription = schedulerRequest.MaterialStatusDescription,
                    Sub1 = schedulerRequest.Sub1,
                    Sub2 = schedulerRequest.Sub2,
                    CreationDate = schedulerRequest.CreationDate,
                    LastModificationDate = schedulerRequest.LastModificationDate,
                    PackageTypeDescription = schedulerRequest.PackageTypeDescription,
                    AreaDescription = schedulerRequest.AreaDescription,

                    ItemUnitMeasure = schedulerRequest.ItemUnitMeasure,
                    Type = (SchedulerRequestType)schedulerRequest.Type,
                    Status = (SchedulerRequestStatus)schedulerRequest.Status,
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return await this.schedulerRequestsDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return new List<object>();
            }
        }

        #endregion
    }
}
