using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BusinessProviders
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
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            string searchString = null)
        {
            var missions = await this.missionsDataService
                .GetAllAsync(
                    skip,
                    take,
                    whereExpression?.ToString(),
                    orderBy.ToQueryString(),
                    searchString);

            return missions
                .Select(m => new Mission
                {
                    Lot = m.Lot,
                    Id = m.Id,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = (MissionStatus)m.Status,
                    Type = (MissionType)m.Type,

                    CreationDate = m.CreationDate,
                    LastModificationDate = m.LastModificationDate,
                    BayDescription = m.BayDescription,
                    ItemDescription = m.ItemDescription,
                    ItemListDescription = m.ItemListDescription,
                    ItemListRowDescription = m.ItemListRowCode,
                    LoadingUnitDescription = m.LoadingUnitCode,
                    Priority = m.Priority,
                    RequiredQuantity = m.RequiredQuantity,
                    CellDescription = m.CellAisleName,
                    CompartmentType = string.Format(Common.Resources.MasterData.CompartmentTypeListFormatReduced, m.CompartmentTypeWidth, m.CompartmentTypeHeight),
                    ItemUnitMeasure = m.ItemMeasureUnitDescription,
                    MaterialStatusDescription = m.MaterialStatusDescription,
                    PackageTypeDescription = m.PackageTypeDescription
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, string searchString = null)
        {
            return await this.missionsDataService.GetAllCountAsync(whereExpression?.ToString(), searchString);
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            var mission = await this.missionsDataService.GetByIdAsync(id);

            return new Mission
            {
                Lot = mission.Lot,
                Id = mission.Id,
                Sub1 = mission.Sub1,
                Sub2 = mission.Sub2,
                RegistrationNumber = mission.RegistrationNumber,
                Status = (MissionStatus)mission.Status,
                Type = (MissionType)mission.Type,

                CreationDate = mission.CreationDate,
                LastModificationDate = mission.LastModificationDate,
                BayDescription = mission.BayDescription,
                ItemDescription = mission.ItemDescription,
                ItemListDescription = mission.ItemListDescription,
                ItemListRowDescription = mission.ItemListRowCode,
                LoadingUnitDescription = mission.LoadingUnitCode,
                Priority = mission.Priority,
                RequiredQuantity = mission.RequiredQuantity,
                CellDescription = mission.CellAisleName,
                CompartmentType = string.Format(Common.Resources.MasterData.CompartmentTypeListFormatReduced, mission.CompartmentTypeWidth, mission.CompartmentTypeHeight),
                ItemUnitMeasure = mission.ItemMeasureUnitDescription,
                MaterialStatusDescription = mission.MaterialStatusDescription,
                PackageTypeDescription = mission.PackageTypeDescription
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.missionsDataService.GetUniqueValuesAsync(propertyName);
        }

        #endregion
    }
}
