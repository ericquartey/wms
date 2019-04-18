﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

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
                    RequestedQuantity = m.RequestedQuantity,
                    CellDescription = m.CellAisleName,
                    CompartmentType = string.Format(Common.Resources.General.CompartmentTypeListFormatReduced, m.CompartmentTypeWidth, m.CompartmentTypeHeight),
                    ItemUnitMeasure = m.ItemMeasureUnitDescription,
                    MaterialStatusDescription = m.MaterialStatusDescription,
                    PackageTypeDescription = m.PackageTypeDescription,
                    DispatchedQuantity = m.DispatchedQuantity
                });
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.missionsDataService.GetAllCountAsync(whereString, searchString);
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
                RequestedQuantity = mission.RequestedQuantity,
                CellDescription = mission.CellAisleName,
                CompartmentType = string.Format(Common.Resources.General.CompartmentTypeListFormatReduced, mission.CompartmentTypeWidth, mission.CompartmentTypeHeight),
                ItemUnitMeasure = mission.ItemMeasureUnitDescription,
                MaterialStatusDescription = mission.MaterialStatusDescription,
                PackageTypeDescription = mission.PackageTypeDescription,
                DispatchedQuantity = mission.DispatchedQuantity
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.missionsDataService.GetUniqueValuesAsync(propertyName);
        }

        #endregion
    }
}
