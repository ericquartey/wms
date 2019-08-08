using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

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
                        AreaName = r.AreaName,
                        BayDescription = r.BayDescription,
                        CreationDate = r.CreationDate,
                        Id = r.Id,
                        IsInstant = r.IsInstant,
                        ItemDescription = r.ItemDescription,
                        LastModificationDate = r.LastModificationDate,
                        ListDescription = r.ListDescription,
                        ListRowCode = r.ListRowCode,
                        LoadingUnitCode = r.LoadingUnitCode,
                        Lot = r.Lot,
                        MaterialStatusDescription = r.MaterialStatusDescription,
                        MeasureUnitDescription = r.MeasureUnitDescription,
                        OperationType = (Enums.OperationType)r.OperationType,
                        PackageTypeDescription = r.PackageTypeDescription,
                        RegistrationNumber = r.RegistrationNumber,
                        RequestedQuantity = r.RequestedQuantity,
                        ReservedQuantity = r.ReservedQuantity,
                        Status = (Enums.SchedulerRequestStatus)r.Status,
                        Sub1 = r.Sub1,
                        Sub2 = r.Sub2,
                        Type = (Enums.SchedulerRequestType)r.Type,
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
                    AreaName = schedulerRequest.AreaName,
                    BayDescription = schedulerRequest.BayDescription,
                    CreationDate = schedulerRequest.CreationDate,
                    IsInstant = schedulerRequest.IsInstant,
                    ItemDescription = schedulerRequest.ItemDescription,
                    LastModificationDate = schedulerRequest.LastModificationDate,
                    ListDescription = schedulerRequest.ListDescription,
                    ListRowCode = schedulerRequest.ListRowCode,
                    LoadingUnitCode = schedulerRequest.LoadingUnitCode,
                    Lot = schedulerRequest.Lot,
                    MaterialStatusDescription = schedulerRequest.MaterialStatusDescription,
                    MeasureUnitDescription = schedulerRequest.MeasureUnitDescription,
                    OperationType = (Enums.OperationType)schedulerRequest.OperationType,
                    PackageTypeDescription = schedulerRequest.PackageTypeDescription,
                    RegistrationNumber = schedulerRequest.RegistrationNumber,
                    RequestedQuantity = schedulerRequest.RequestedQuantity,
                    ReservedQuantity = schedulerRequest.ReservedQuantity,
                    Status = (Enums.SchedulerRequestStatus)schedulerRequest.Status,
                    Sub1 = schedulerRequest.Sub1,
                    Sub2 = schedulerRequest.Sub2,
                    Type = (Enums.SchedulerRequestType)schedulerRequest.Type,
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
