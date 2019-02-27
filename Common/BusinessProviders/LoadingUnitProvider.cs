using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BusinessProviders
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
                var item = await this.loadingUnitsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.LoadingUnitDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    AbcClassId = model.AbcClassId,
                    AbcClassDescription = model.AbcClassDescription,
                    CellPositionId = model.CellPositionId,
                    CellPositionDescription = model.CellPositionDescription,
                    LoadingUnitStatusId = model.LoadingUnitStatusId,
                    LoadingUnitStatusDescription = model.LoadingUnitStatusDescription,
                    LoadingUnitTypeId = model.LoadingUnitTypeId,
                    LoadingUnitTypeDescription = model.LoadingUnitTypeDescription,
                    Width = model.Width,
                    Length = model.Length,
                    Note = model.Note,
                    IsCellPairingFixed = model.IsCellPairingFixed,
                    ReferenceType = (WMS.Data.WebAPI.Contracts.ReferenceType)model.ReferenceType,
                    Height = model.Height,
                    Weight = model.Weight,
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
                });

                model.Id = item.Id;

                return new OperationResult<LoadingUnitDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<LoadingUnitDetails>(ex);
            }
        }

        public async Task<IEnumerable<LoadingUnit>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            string whereExpression = null,
            string searchString = null)
        {
            var loadingUnits = await this.loadingUnitsDataService
                .GetAllAsync(skip, take, whereExpression, orderBy.ToQueryString(), searchString);

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
                    CellSide = (Side)l.CellSide,
                    CellNumber = l.CellNumber,
                    CellPositionDescription = l.CellPositionDescription,
                });
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
                    Id = l.Id,
                    Code = l.Code,
                    AbcClassId = l.AbcClassId,
                    AbcClassDescription = l.AbcClassDescription,
                    CellPositionId = l.CellPositionId,
                    CellPositionDescription = l.CellPositionDescription,
                    LoadingUnitStatusId = l.LoadingUnitStatusId,
                    LoadingUnitStatusDescription = l.LoadingUnitStatusDescription,
                    LoadingUnitTypeId = l.LoadingUnitTypeId,
                    LoadingUnitTypeDescription = l.LoadingUnitTypeDescription,
                    Width = l.Width,
                    Length = l.Length,
                    Note = l.Note,
                    IsCellPairingFixed = l.IsCellPairingFixed,
                    ReferenceType = (ReferenceType)l.ReferenceType,
                    Height = l.Height,
                    Weight = l.Weight,
                    HandlingParametersCorrection = l.HandlingParametersCorrection,
                    LoadingUnitTypeHasCompartments = l.LoadingUnitTypeHasCompartments,
                    CreationDate = l.CreationDate,
                    LastHandlingDate = l.LastHandlingDate,
                    InventoryDate = l.InventoryDate,
                    LastPickDate = l.LastPickDate,
                    LastStoreDate = l.LastStoreDate,
                    InCycleCount = l.InCycleCount,
                    OutCycleCount = l.OutCycleCount,
                    OtherCycleCount = l.OtherCycleCount,
                    CellId = l.CellId,
                    AisleId = l.AisleId,
                    AreaId = l.AreaId,
                    CompartmentsCount = l.CompartmentsCount
                });
        }

        public async Task<LoadingUnitDetails> GetByIdAsync(int id)
        {
            var loadingUnit = await this.loadingUnitsDataService.GetByIdAsync(id);

            var abcClassChoices = await this.abcClassProvider.GetAllAsync();
            var cellPositionChoices = await this.cellPositionProvider.GetAllAsync();
            var loadingUnitStatusChoices = await this.loadingUnitStatusProvider.GetAllAsync();
            var loadingUnitTypeChoices = await this.loadingUnitTypeProvider.GetAllAsync();
            var cellChoices = await this.GetCellsByAreaIdAsync(loadingUnit.AreaId);

            var l = new LoadingUnitDetails
            {
                Id = loadingUnit.Id,
                Code = loadingUnit.Code,
                AbcClassId = loadingUnit.AbcClassId,
                AbcClassDescription = loadingUnit.AbcClassDescription,
                CellPositionId = loadingUnit.CellPositionId,
                CellPositionDescription = loadingUnit.CellPositionDescription,
                LoadingUnitStatusId = loadingUnit.LoadingUnitStatusId,
                LoadingUnitStatusDescription = loadingUnit.LoadingUnitStatusDescription,
                LoadingUnitTypeId = loadingUnit.LoadingUnitTypeId,
                LoadingUnitTypeDescription = loadingUnit.LoadingUnitTypeDescription,
                Width = loadingUnit.Width,
                Length = loadingUnit.Length,
                Note = loadingUnit.Note,
                IsCellPairingFixed = loadingUnit.IsCellPairingFixed,
                ReferenceType = (ReferenceType)loadingUnit.ReferenceType,
                Height = loadingUnit.Height,
                Weight = loadingUnit.Weight,
                HandlingParametersCorrection = loadingUnit.HandlingParametersCorrection,
                LoadingUnitTypeHasCompartments = loadingUnit.LoadingUnitTypeHasCompartments,
                CreationDate = loadingUnit.CreationDate,
                LastHandlingDate = loadingUnit.LastHandlingDate,
                InventoryDate = loadingUnit.InventoryDate,
                LastPickDate = loadingUnit.LastPickDate,
                LastStoreDate = loadingUnit.LastStoreDate,
                InCycleCount = loadingUnit.InCycleCount,
                OutCycleCount = loadingUnit.OutCycleCount,
                OtherCycleCount = loadingUnit.OtherCycleCount,
                CellId = loadingUnit.CellId,
                AisleId = loadingUnit.AisleId,
                AreaId = loadingUnit.AreaId,
                CompartmentsCount = loadingUnit.CompartmentsCount,

                AbcClassChoices = abcClassChoices,
                CellPositionChoices = cellPositionChoices,
                LoadingUnitStatusChoices = loadingUnitStatusChoices,
                LoadingUnitTypeChoices = loadingUnitTypeChoices,
                CellChoices = cellChoices
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
                    FifoTime = c.FifoTime,
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
                    LastHandlingDate = c.LastHandlingDate,
                    InventoryDate = c.InventoryDate,
                    FirstStoreDate = c.FirstStoreDate,
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
                    ItemMeasureUnit = c.ItemMeasureUnit
                });
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
                await this.loadingUnitsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.LoadingUnitDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    AbcClassId = model.AbcClassId,
                    AbcClassDescription = model.AbcClassDescription,
                    CellPositionId = model.CellPositionId,
                    CellPositionDescription = model.CellPositionDescription,
                    LoadingUnitStatusId = model.LoadingUnitStatusId,
                    LoadingUnitStatusDescription = model.LoadingUnitStatusDescription,
                    LoadingUnitTypeId = model.LoadingUnitTypeId,
                    LoadingUnitTypeDescription = model.LoadingUnitTypeDescription,
                    Width = model.Width,
                    Length = model.Length,
                    Note = model.Note,
                    IsCellPairingFixed = model.IsCellPairingFixed,
                    ReferenceType = (WMS.Data.WebAPI.Contracts.ReferenceType)model.ReferenceType,
                    Height = model.Height,
                    Weight = model.Weight,
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
                });

                return new OperationResult<LoadingUnitDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<LoadingUnitDetails>(ex);
            }
        }

        #endregion
    }
}
