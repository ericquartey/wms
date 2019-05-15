﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "Ok")]
    public class LoadingUnitProvider : ILoadingUnitProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly ICellPositionProvider cellPositionProvider;

        private readonly WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService;

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService;

        private readonly ILoadingUnitStatusProvider loadingUnitStatusProvider;

        private readonly ILoadingUnitTypeProvider loadingUnitTypeProvider;

        #endregion

        #region Constructors

        public LoadingUnitProvider(
            IAbcClassProvider abcClassProvider,
            ICellPositionProvider cellPositionProvider,
            ILoadingUnitStatusProvider loadingUnitStatusProvider,
            ILoadingUnitTypeProvider loadingUnitTypeProvider,
            WMS.Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService,
            WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.abcClassProvider = abcClassProvider;
            this.cellPositionProvider = cellPositionProvider;
            this.loadingUnitStatusProvider = loadingUnitStatusProvider;
            this.loadingUnitTypeProvider = loadingUnitTypeProvider;
            this.loadingUnitsDataService = loadingUnitsDataService;
            this.cellsDataService = cellsDataService;
            this.areasDataService = areasDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<LoadingUnitDetails>> CreateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var loadingUnit = await this.loadingUnitsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.LoadingUnitCreating
                {
                    Id = model.Id,
                    Code = model.Code,
                    AbcClassId = model.AbcClassId,
                    CellPositionId = model.CellPositionId,
                    LoadingUnitStatusId = model.LoadingUnitStatusId,
                    LoadingUnitTypeId = model.LoadingUnitTypeId.GetValueOrDefault(),
                    Note = model.Note,
                    IsCellPairingFixed = model.IsCellPairingFixed,
                    ReferenceType = (WMS.Data.WebAPI.Contracts.ReferenceType)model.ReferenceType,
                    Height = model.Height.GetValueOrDefault(),
                    Weight = model.Weight.GetValueOrDefault(),
                    InCycleCount = model.InCycleCount,
                    OutCycleCount = model.OutCycleCount,
                    OtherCycleCount = model.OtherCycleCount,
                    CellId = model.CellId,
                    AisleId = model.AisleId,
                    AreaId = model.AreaId,
                    HandlingParametersCorrection = model.HandlingParametersCorrection,
                });

                model.Id = loadingUnit.Id;
                return new OperationResult<LoadingUnitDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<LoadingUnitDetails>(ex);
            }
        }

        public async Task<IOperationResult<LoadingUnit>> DeleteAsync(int id)
        {
            try
            {
                await this.loadingUnitsDataService.DeleteAsync(id);

                return new OperationResult<LoadingUnit>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<LoadingUnit>(ex);
            }
        }

        public async Task<IEnumerable<LoadingUnit>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var loadingUnits = await this.loadingUnitsDataService
                .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

            return loadingUnits
                .Select(l => new LoadingUnit
                {
                    Id = l.Id,
                    Code = l.Code,
                    LoadingUnitTypeDescription = l.LoadingUnitTypeDescription,
                    LoadingUnitStatusDescription = l.LoadingUnitStatusDescription,
                    AbcClassDescription = l.AbcClassDescription,
                    AreaName = l.AreaName,
                    AisleName = l.AisleName,
                    CellFloor = l.CellFloor,
                    CellColumn = l.CellColumn,
                    CellSide = (Side?)l.CellSide,
                    CellNumber = l.CellNumber,
                    CellPositionDescription = l.CellPositionDescription,
                    AreaFillRate = l.AreaFillRate.GetValueOrDefault(),
                    Policies = l.GetPolicies(),
                });
        }

        public async Task<IEnumerable<Enumeration>> GetAllCellsAsync()
        {
            return (await this.cellsDataService.GetAllAsync())
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.loadingUnitsDataService
                .GetAllCountAsync(whereString, searchString);
        }

        public async Task<IEnumerable<LoadingUnitDetails>> GetByCellIdAsync(int id)
        {
            return (await this.cellsDataService.GetLoadingUnitsAsync(id))
                .Select(l => new LoadingUnitDetails
                {
                    AbcClassDescription = l.AbcClassDescription,
                    AbcClassId = l.AbcClassId,
                    AisleId = l.AisleId,
                    AreaId = l.AreaId,
                    CellId = l.CellId,
                    CellPositionDescription = l.CellPositionDescription,
                    CellPositionId = l.CellPositionId,
                    Code = l.Code,
                    CompartmentsCount = l.CompartmentsCount,
                    CreationDate = l.CreationDate,
                    HandlingParametersCorrection = l.HandlingParametersCorrection,
                    Height = l.Height,
                    Id = l.Id,
                    InCycleCount = l.InCycleCount,
                    InventoryDate = l.InventoryDate,
                    IsCellPairingFixed = l.IsCellPairingFixed,
                    LastHandlingDate = l.LastHandlingDate,
                    LastPickDate = l.LastPickDate,
                    LastStoreDate = l.LastStoreDate,
                    Length = l.Length,
                    LoadingUnitStatusDescription = l.LoadingUnitStatusDescription,
                    LoadingUnitStatusId = l.LoadingUnitStatusId,
                    LoadingUnitTypeDescription = l.LoadingUnitTypeDescription,
                    LoadingUnitTypeHasCompartments = l.LoadingUnitTypeHasCompartments,
                    LoadingUnitTypeId = l.LoadingUnitTypeId,
                    Note = l.Note,
                    OtherCycleCount = l.OtherCycleCount,
                    OutCycleCount = l.OutCycleCount,
                    Policies = l.GetPolicies(),
                    ReferenceType = (ReferenceType)l.ReferenceType,
                    Weight = l.Weight,
                    Width = l.Width
                });
        }

        public async Task<LoadingUnitDetails> GetByIdAsync(int id)
        {
            var loadingUnit = await this.loadingUnitsDataService.GetByIdAsync(id);

            var loadingUnitEnumeration = new LoadingUnitDetails();
            await this.AddEnumerationsAsync(loadingUnitEnumeration);

            IEnumerable<Enumeration> cellChoices;
            if (loadingUnit.AreaId.HasValue)
            {
                cellChoices = await this.GetCellsByAreaIdAsync(loadingUnit.AreaId.GetValueOrDefault());
            }
            else
            {
                cellChoices = await this.GetAllCellsAsync();
            }

            var l = new LoadingUnitDetails
            {
                AbcClassChoices = loadingUnitEnumeration.AbcClassChoices,
                AbcClassDescription = loadingUnit.AbcClassDescription,
                AbcClassId = loadingUnit.AbcClassId,
                AisleId = loadingUnit.AisleId,
                AreaId = loadingUnit.AreaId,
                AreaName = loadingUnit.AreaName,
                CellChoices = cellChoices,
                CellId = loadingUnit.CellId,
                CellPositionChoices = loadingUnitEnumeration.CellPositionChoices,
                CellPositionDescription = loadingUnit.CellPositionDescription,
                CellPositionId = loadingUnit.CellPositionId,
                Code = loadingUnit.Code,
                CompartmentsCount = loadingUnit.CompartmentsCount,
                CreationDate = loadingUnit.CreationDate,
                HandlingParametersCorrection = loadingUnit.HandlingParametersCorrection,
                Height = loadingUnit.Height,
                Id = loadingUnit.Id,
                InCycleCount = loadingUnit.InCycleCount,
                InventoryDate = loadingUnit.InventoryDate,
                IsCellPairingFixed = loadingUnit.IsCellPairingFixed,
                LastHandlingDate = loadingUnit.LastHandlingDate,
                LastPickDate = loadingUnit.LastPickDate,
                LastStoreDate = loadingUnit.LastStoreDate,
                Length = loadingUnit.Length,
                LoadingUnitStatusChoices = loadingUnitEnumeration.LoadingUnitStatusChoices,
                LoadingUnitStatusDescription = loadingUnit.LoadingUnitStatusDescription,
                LoadingUnitStatusId = loadingUnit.LoadingUnitStatusId,
                LoadingUnitTypeChoices = loadingUnitEnumeration.LoadingUnitTypeChoices,
                LoadingUnitTypeDescription = loadingUnit.LoadingUnitTypeDescription,
                LoadingUnitTypeHasCompartments = loadingUnit.LoadingUnitTypeHasCompartments,
                LoadingUnitTypeId = loadingUnit.LoadingUnitTypeId,
                Note = loadingUnit.Note,
                OtherCycleCount = loadingUnit.OtherCycleCount,
                OutCycleCount = loadingUnit.OutCycleCount,
                Policies = loadingUnit.GetPolicies(),
                ReferenceType = (ReferenceType)loadingUnit.ReferenceType,
                Weight = loadingUnit.Weight,
                Width = loadingUnit.Width,
            };

            foreach (var compartment in await this.GetCompartmentsByLoadingUnitIdAsync(id))
            {
                l.AddCompartment(compartment);
            }

            return l;
        }

        public async Task<IEnumerable<Enumeration>> GetCellsByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetCellsAsync(areaId))
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<IEnumerable<CompartmentDetails>> GetCompartmentsByLoadingUnitIdAsync(int id)
        {
            return (await this.loadingUnitsDataService.GetCompartmentsAsync(id))
                .Select(c => new CompartmentDetails
                {
                    Id = c.Id,
                    LoadingUnitCode = c.LoadingUnitCode,
                    CompartmentTypeId = c.CompartmentTypeId,
                    ItemCode = c.ItemCode,
                    ItemDescription = c.ItemDescription,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    MaterialStatusId = c.MaterialStatusId,
                    PackageTypeId = c.PackageTypeId,
                    Lot = c.Lot,
                    RegistrationNumber = c.RegistrationNumber,
                    MaxCapacity = c.MaxCapacity,
                    Stock = c.Stock,
                    ReservedForPick = c.ReservedForPick,
                    ReservedToStore = c.ReservedToStore,
                    CompartmentStatusId = c.CompartmentStatusId,
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    CreationDate = c.CreationDate,
                    InventoryDate = c.InventoryDate,
                    FifoStartDate = c.FifoStartDate,
                    LastStoreDate = c.LastStoreDate,
                    LastPickDate = c.LastPickDate,
                    Width = c.HasRotation ? c.Height : c.Width,
                    Height = c.HasRotation ? c.Width : c.Height,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                    LoadingUnitId = c.LoadingUnitId,
                    ItemId = c.ItemId,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    LoadingUnitHasCompartments = c.LoadingUnitHasCompartments,
                    ItemMeasureUnit = c.ItemMeasureUnit,
                    Policies = c.GetPolicies(),
                });
        }

        public async Task<LoadingUnitDetails> GetNewAsync()
        {
            var loadingUnitDetails = new LoadingUnitDetails();
            loadingUnitDetails.Length = 1;
            await this.AddEnumerationsAsync(loadingUnitDetails);
            return loadingUnitDetails;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.loadingUnitsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.loadingUnitsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.LoadingUnitDetails
                    {
                        Id = model.Id,
                        Code = model.Code,
                        AbcClassId = model.AbcClassId,
                        AbcClassDescription = model.AbcClassDescription,
                        CellPositionId = model.CellPositionId,
                        CellPositionDescription = model.CellPositionDescription,
                        LoadingUnitStatusId = model.LoadingUnitStatusId,
                        LoadingUnitStatusDescription = model.LoadingUnitStatusDescription,
                        LoadingUnitTypeId = model.LoadingUnitTypeId.GetValueOrDefault(),
                        LoadingUnitTypeDescription = model.LoadingUnitTypeDescription,
                        Width = model.Width,
                        Length = model.Length,
                        Note = model.Note,
                        IsCellPairingFixed = model.IsCellPairingFixed,
                        ReferenceType = (WMS.Data.WebAPI.Contracts.ReferenceType)model.ReferenceType,
                        Height = model.Height.GetValueOrDefault(),
                        Weight = model.Weight.GetValueOrDefault(),
                        HandlingParametersCorrection = model.HandlingParametersCorrection,
                        LoadingUnitTypeHasCompartments = model.LoadingUnitTypeHasCompartments,
                        CreationDate = model.CreationDate,
                        LastHandlingDate = model.LastHandlingDate,
                        InventoryDate = model.InventoryDate,
                        LastPickDate = model.LastPickDate,
                        LastStoreDate = model.LastStoreDate,
                        InCycleCount = model.InCycleCount,
                        OutCycleCount = model.OutCycleCount,
                        OtherCycleCount = model.OtherCycleCount,
                        CellId = model.CellId,
                        AisleId = model.AisleId,
                        AreaId = model.AreaId,
                        CompartmentsCount = model.CompartmentsCount
                    },
                    model.Id);

                return new OperationResult<LoadingUnitDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<LoadingUnitDetails>(ex);
            }
        }

        public async Task<IOperationResult<SchedulerRequest>> WithdrawAsync(int loadingUnitId, int bayId)
        {
            try
            {
                var request = await this.loadingUnitsDataService.WithdrawAsync(loadingUnitId, bayId);
                return new OperationResult<SchedulerRequest>(request != null);
            }
            catch (Exception ex)
            {
                return new OperationResult<SchedulerRequest>(ex);
            }
        }

        private async Task AddEnumerationsAsync(LoadingUnitDetails loadingUnitDetails)
        {
            if (loadingUnitDetails != null)
            {
                loadingUnitDetails.AbcClassChoices = await this.abcClassProvider.GetAllAsync();
                loadingUnitDetails.CellPositionChoices = await this.cellPositionProvider.GetAllAsync();
                loadingUnitDetails.LoadingUnitStatusChoices = await this.loadingUnitStatusProvider.GetAllAsync();
                loadingUnitDetails.LoadingUnitTypeChoices = await this.loadingUnitTypeProvider.GetAllAsync();
            }
        }

        #endregion
    }
}
