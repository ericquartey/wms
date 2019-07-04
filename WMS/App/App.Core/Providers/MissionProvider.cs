using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Providers
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        #endregion

        #region Constructors

        public MissionProvider(
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService)
        {
            this.missionsDataService = missionsDataService;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> GetAllAsync(
           int skip,
           int take,
           IEnumerable<SortOption> orderBySortOptions = null,
           string whereString = null,
           string searchString = null)
        {
            try
            {
                var missions = await this.missionsDataService
                    .GetAllAsync(
                        skip,
                        take,
                        whereString,
                        orderBySortOptions.ToQueryString(),
                        searchString);

                return missions
                    .Select(m => new Mission
                    {
                        Id = m.Id,
                        Status = (MissionStatus)m.Status,
                        CreationDate = m.CreationDate,
                        LastModificationDate = m.LastModificationDate,
                        BayDescription = m.BayDescription,
                        LoadingUnitDescription = m.LoadingUnitCode,
                        Priority = m.Priority,
                        Operations = m.Operations.Select(o =>
                        new MissionOperation
                        {
                            Lot = o.Lot,
                            Sub1 = o.Sub1,
                            Sub2 = o.Sub2,
                            MissionId = o.MissionId,
                            RegistrationNumber = o.RegistrationNumber,
                            Type = (MissionOperationType)o.Type,
                            ItemDescription = o.ItemDescription,
                            ItemListDescription = o.ItemListDescription,
                            ItemListRowDescription = o.ItemListRowCode,
                            RequestedQuantity = o.RequestedQuantity,
                            CreationDate = o.CreationDate,
                            LastModificationDate = o.LastModificationDate,
                            Status = (MissionOperationStatus)o.Status,
                            CompartmentType = string.Format(General.CompartmentTypeListFormatReduced, o.CompartmentWidth, o.CompartmentDepth),
                            ItemMeasureUnitDescription = o.ItemMeasureUnitDescription,
                            MaterialStatusDescription = o.MaterialStatusDescription,
                            PackageTypeDescription = o.PackageTypeDescription,
                            DispatchedQuantity = o.DispatchedQuantity,
                        })
                    });
            }
            catch
            {
                return new List<Mission>();
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.missionsDataService.GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            try
            {
                var mission = await this.missionsDataService.GetByIdAsync(id);

                return new Mission
                {
                    Id = mission.Id,
                    Status = (MissionStatus)mission.Status,
                    CreationDate = mission.CreationDate,
                    LastModificationDate = mission.LastModificationDate,
                    BayDescription = mission.BayDescription,
                    LoadingUnitDescription = mission.LoadingUnitCode,
                    Priority = mission.Priority,
                    Operations = mission.Operations.Select(o =>
                        new MissionOperation
                        {
                            Lot = o.Lot,
                            Sub1 = o.Sub1,
                            Sub2 = o.Sub2,
                            RegistrationNumber = o.RegistrationNumber,
                            Type = (MissionOperationType)o.Type,
                            CreationDate = o.CreationDate,
                            LastModificationDate = o.LastModificationDate,
                            ItemDescription = o.ItemDescription,
                            ItemListDescription = o.ItemListDescription,
                            ItemListRowDescription = o.ItemListRowCode,
                            RequestedQuantity = o.RequestedQuantity,
                            CompartmentType = string.Format(General.CompartmentTypeListFormatReduced, o.CompartmentWidth, o.CompartmentDepth),
                            MaterialStatusDescription = o.MaterialStatusDescription,
                            PackageTypeDescription = o.PackageTypeDescription,
                            DispatchedQuantity = o.DispatchedQuantity,
                        })
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
                return await this.missionsDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return new List<object>();
            }
        }

        #endregion
    }
}
