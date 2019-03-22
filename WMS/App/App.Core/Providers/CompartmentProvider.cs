using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "Ok")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "Ok")]
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly ICellPositionProvider cellPositionProvider;

        private readonly WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService;

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService;

        private readonly ICompartmentStatusProvider compartmentStatusProvider;

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService;

        private readonly ILoadingUnitStatusProvider loadingUnitStatusProvider;

        private readonly ILoadingUnitTypeProvider loadingUnitTypeProvider;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public CompartmentProvider(
            ICompartmentStatusProvider compartmentStatusProvider,
            ICompartmentTypeProvider compartmentTypeProvider,
            IPackageTypeProvider packageTypeProvider,
            IMaterialStatusProvider materialStatusProvider,
            IAbcClassProvider abcClassProvider,
            ICellPositionProvider cellPositionProvider,
            ILoadingUnitStatusProvider loadingUnitStatusProvider,
            ILoadingUnitTypeProvider loadingUnitTypeProvider,
            WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService,
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService,
            WMS.Data.WebAPI.Contracts.ICellsDataService cellsDataService)
        {
            this.compartmentsDataService = compartmentsDataService;
            this.itemsDataService = itemsDataService;
            this.loadingUnitsDataService = loadingUnitsDataService;
            this.areasDataService = areasDataService;
            this.cellsDataService = cellsDataService;

            this.abcClassProvider = abcClassProvider;
            this.cellPositionProvider = cellPositionProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.compartmentStatusProvider = compartmentStatusProvider;
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
            this.loadingUnitStatusProvider = loadingUnitStatusProvider;
            this.loadingUnitTypeProvider = loadingUnitTypeProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ICompartment>> AddRangeAsync(IEnumerable<ICompartment> compartments)
        {
            if (compartments == null)
            {
                throw new ArgumentNullException(nameof(compartments));
            }

            try
            {
                var compartmentsApi = new List<WMS.Data.WebAPI.Contracts.CompartmentDetails>();
                foreach (var compartment in compartments.Cast<CompartmentDetails>())
                {
                    compartmentsApi.Add(new WMS.Data.WebAPI.Contracts.CompartmentDetails
                    {
                        CompartmentStatusId = compartment.CompartmentStatusId,
                        CompartmentTypeId = compartment.CompartmentTypeId,
                        CreationDate = DateTime.Now,
                        Height = compartment.Height,
                        IsItemPairingFixed = compartment.IsItemPairingFixed,
                        ItemId = compartment.ItemId,
                        LoadingUnitId = compartment.LoadingUnitId,
                        Lot = compartment.Lot,
                        MaterialStatusId = compartment.MaterialStatusId,
                        MaxCapacity = compartment.MaxCapacity,
                        PackageTypeId = compartment.PackageTypeId,
                        RegistrationNumber = compartment.RegistrationNumber,
                        ReservedForPick = compartment.ReservedForPick,
                        ReservedToStore = compartment.ReservedToStore,
                        Stock = compartment.Stock,
                        Sub1 = compartment.Sub1,
                        Sub2 = compartment.Sub2,
                        Width = compartment.Width,
                        XPosition = compartment.XPosition,
                        YPosition = compartment.YPosition,
                    });
                }

                await this.compartmentsDataService.CreateRangeAsync(compartmentsApi);

                return new OperationResult<CompartmentDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentDetails>(ex);
            }
        }

        public async Task<ActionModel> CanDeleteAsync(int id)
        {
            var action = await this.compartmentsDataService.CanDeleteAsync(id);
            return new ActionModel
            {
                IsAllowed = action.IsAllowed,
                Reason = action.Reason,
            };
        }

        public async Task<IOperationResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var compartment = await this.compartmentsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.CompartmentDetails
                {
                    CompartmentStatusId = model.CompartmentStatusId,
                    CompartmentTypeId = model.CompartmentTypeId,
                    CreationDate = DateTime.Now,
                    Height = model.Height,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    ItemId = model.ItemId,
                    LoadingUnitId = model.LoadingUnitId,
                    Lot = model.Lot,
                    MaterialStatusId = model.MaterialStatusId,
                    MaxCapacity = model.MaxCapacity,
                    PackageTypeId = model.PackageTypeId,
                    RegistrationNumber = model.RegistrationNumber,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    Stock = model.Stock,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    Width = model.Width,
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                });

                model.Id = compartment.Id;

                model.LoadingUnit?.Compartments.Add(model);

                return new OperationResult<CompartmentDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentDetails>(ex);
            }
        }

        public async Task<IOperationResult<CompartmentDetails>> DeleteAsync(int id)
        {
            try
            {
                await this.compartmentsDataService.DeleteAsync(id);

                return new OperationResult<CompartmentDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentDetails>(ex);
            }
        }

        public async Task<IEnumerable<Compartment>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return (await this.compartmentsDataService.GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString))
                .Select(c => new Compartment
                {
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    CompartmentTypeDescription = string.Format(
                        Common.Resources.MasterData.CompartmentTypeListFormatReduced,
                        c.HasRotation ? c.Width : c.Height,
                        c.HasRotation ? c.Height : c.Width),
                    Id = c.Id,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemDescription = c.ItemDescription,
                    ItemMeasureUnit = c.ItemMeasureUnit,
                    LoadingUnitCode = c.LoadingUnitCode,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatusDescription,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                });
        }

        public async Task<IEnumerable<Enumeration>> GetAllCellsAsync()
        {
            return (await this.cellsDataService.GetAllAsync(null, null, null, null, null))
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.compartmentsDataService.GetAllCountAsync(whereString, searchString);
        }

        public async Task<CompartmentDetails> GetByIdAsync(int id)
        {
            var compartment = await this.compartmentsDataService.GetByIdAsync(id);
            var compartmentStatusChoices = await this.compartmentStatusProvider.GetAllAsync();
            var compartmentTypeChoices = await this.compartmentTypeProvider.GetAllAsync();
            var materialStatusChoices = await this.materialStatusProvider.GetAllAsync();
            var packageTypeChoices = await this.packageTypeProvider.GetAllAsync();
            var loadingUnit = await this.GetLoadingUnitByIdAsync(compartment.LoadingUnitId);

            return new CompartmentDetails
            {
                CompartmentStatusChoices = compartmentStatusChoices,
                CompartmentStatusDescription = compartment.CompartmentStatusDescription,
                CompartmentStatusId = compartment.CompartmentStatusId,
                CompartmentTypeChoices = compartmentTypeChoices,
                CompartmentTypeId = compartment.CompartmentTypeId,
                CreationDate = compartment.CreationDate,
                FifoTime = compartment.FifoTime,
                FirstStoreDate = compartment.FirstStoreDate,
                Height = compartment.HasRotation ? compartment.Width : compartment.Height,
                Id = compartment.Id,
                InventoryDate = compartment.InventoryDate,
                IsItemPairingFixed = compartment.IsItemPairingFixed,
                ItemCode = compartment.ItemCode,
                ItemDescription = compartment.ItemDescription,
                ItemId = compartment.ItemId,
                ItemMeasureUnit = compartment.ItemMeasureUnit,
                LastPickDate = compartment.LastPickDate,
                LastStoreDate = compartment.LastStoreDate,
                LoadingUnit = loadingUnit,
                LoadingUnitCode = compartment.LoadingUnitCode,
                LoadingUnitHasCompartments = compartment.LoadingUnitHasCompartments,
                LoadingUnitId = compartment.LoadingUnitId,
                Lot = compartment.Lot,
                MaterialStatusChoices = materialStatusChoices,
                MaterialStatusId = compartment.MaterialStatusId,
                MaxCapacity = compartment.MaxCapacity,
                PackageTypeChoices = packageTypeChoices,
                PackageTypeId = compartment.PackageTypeId,
                RegistrationNumber = compartment.RegistrationNumber,
                ReservedForPick = compartment.ReservedForPick,
                ReservedToStore = compartment.ReservedToStore,
                Stock = compartment.Stock,
                Sub1 = compartment.Sub1,
                Sub2 = compartment.Sub2,
                Width = compartment.HasRotation ? compartment.Height : compartment.Width,
                XPosition = compartment.XPosition,
                YPosition = compartment.YPosition,
            };
        }

        public async Task<IEnumerable<Compartment>> GetByItemIdAsync(int id)
        {
            return (await this.itemsDataService.GetCompartmentsAsync(id))
                .Select(c => new Compartment
                {
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    Id = c.Id,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemDescription = c.ItemDescription,
                    ItemMeasureUnit = c.ItemMeasureUnit,
                    LoadingUnitCode = c.LoadingUnitCode,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatusDescription,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                });
        }

        public async Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id)
        {
            return (await this.loadingUnitsDataService.GetCompartmentsAsync(id))
                .Select(c => new CompartmentDetails
                {
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    CompartmentStatusId = c.CompartmentStatusId,
                    CompartmentTypeId = c.CompartmentTypeId,
                    CreationDate = c.CreationDate,
                    FifoTime = c.FifoTime,
                    FirstStoreDate = c.FirstStoreDate,
                    Height = c.HasRotation ? c.Width : c.Height,
                    Id = c.Id,
                    InventoryDate = c.InventoryDate,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemCode = c.ItemCode,
                    ItemDescription = c.ItemDescription,
                    ItemId = c.ItemId,
                    ItemMeasureUnit = c.ItemMeasureUnit,
                    LastPickDate = c.LastPickDate,
                    LastStoreDate = c.LastStoreDate,
                    LoadingUnitCode = c.LoadingUnitCode,
                    LoadingUnitHasCompartments = c.LoadingUnitHasCompartments,
                    LoadingUnitId = c.LoadingUnitId,
                    Lot = c.Lot,
                    MaterialStatusId = c.MaterialStatusId,
                    MaxCapacity = c.MaxCapacity,
                    PackageTypeId = c.PackageTypeId,
                    RegistrationNumber = c.RegistrationNumber,
                    ReservedForPick = c.ReservedForPick,
                    ReservedToStore = c.ReservedToStore,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    Width = c.HasRotation ? c.Height : c.Width,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                });
        }

        public async Task<IEnumerable<Enumeration>> GetCellsByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetCellsAsync(areaId))
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<LoadingUnitDetails> GetLoadingUnitByIdAsync(int id)
        {
            var loadingUnit = await this.loadingUnitsDataService.GetByIdAsync(id);

            var abcClassChoices = await this.abcClassProvider.GetAllAsync();
            var cellPositionChoices = await this.cellPositionProvider.GetAllAsync();
            var loadingUnitStatusChoices = await this.loadingUnitStatusProvider.GetAllAsync();
            var loadingUnitTypeChoices = await this.loadingUnitTypeProvider.GetAllAsync();
            IEnumerable<Enumeration> cellChoices;
            if (loadingUnit.AreaId.HasValue)
            {
                cellChoices = await this.GetCellsByAreaIdAsync(loadingUnit.AreaId.Value);
            }
            else
            {
                cellChoices = await this.GetAllCellsAsync();
            }

            var l = new LoadingUnitDetails
            {
                AbcClassChoices = abcClassChoices,
                AbcClassDescription = loadingUnit.AbcClassDescription,
                AbcClassId = loadingUnit.AbcClassId,
                AisleId = loadingUnit.AisleId,
                AreaId = loadingUnit.AreaId,
                CellChoices = cellChoices,
                CellId = loadingUnit.CellId,
                CellPositionChoices = cellPositionChoices,
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
                LastPickDate = loadingUnit.LastPickDate,
                LastStoreDate = loadingUnit.LastStoreDate,
                Length = loadingUnit.Length,
                LoadingUnitStatusChoices = loadingUnitStatusChoices,
                LoadingUnitStatusDescription = loadingUnit.LoadingUnitStatusDescription,
                LoadingUnitStatusId = loadingUnit.LoadingUnitStatusId,
                LoadingUnitTypeChoices = loadingUnitTypeChoices,
                LoadingUnitTypeDescription = loadingUnit.LoadingUnitTypeDescription,
                LoadingUnitTypeHasCompartments = loadingUnit.LoadingUnitTypeHasCompartments,
                LoadingUnitTypeId = loadingUnit.LoadingUnitTypeId,
                Note = loadingUnit.Note,
                OtherCycleCount = loadingUnit.OtherCycleCount,
                OutCycleCount = loadingUnit.OutCycleCount,
                ReferenceType = (ReferenceType)loadingUnit.ReferenceType,
                Weight = loadingUnit.Weight,
                Width = loadingUnit.Width,
            };

            foreach (var compartment in await this.GetByLoadingUnitIdAsync(id))
            {
                l.AddCompartment(compartment);
            }

            return l;
        }

        public async Task<int?> GetMaxCapacityAsync(int? width, int? height, int itemId)
        {
            if (width.HasValue && height.HasValue)
            {
                return await this.compartmentsDataService.GetMaxCapacityAsync(width.Value, height.Value, itemId);
            }

            return null;
        }

        public async Task<CompartmentDetails> GetNewAsync()
        {
            var compartmentStatus = await this.compartmentStatusProvider.GetAllAsync();
            var compartmentType = await this.compartmentTypeProvider.GetAllAsync();
            var packageType = await this.packageTypeProvider.GetAllAsync();
            var materialStatus = await this.materialStatusProvider.GetAllAsync();
            return new CompartmentDetails
            {
                CompartmentStatusChoices = compartmentStatus,
                CompartmentTypeChoices = compartmentType,
                MaterialStatusChoices = materialStatus,
                PackageTypeChoices = packageType
            };
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.compartmentsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.compartmentsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.CompartmentDetails
                {
                    CompartmentStatusDescription = model.CompartmentStatusDescription,
                    CompartmentStatusId = model.CompartmentStatusId,
                    CompartmentTypeId = model.CompartmentTypeId,
                    CreationDate = model.CreationDate,
                    FifoTime = model.FifoTime,
                    FirstStoreDate = model.FirstStoreDate,
                    Height = model.Height,
                    Id = model.Id,
                    InventoryDate = model.InventoryDate,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    ItemCode = model.ItemCode,
                    ItemDescription = model.ItemDescription,
                    ItemId = model.ItemId,
                    ItemMeasureUnit = model.ItemMeasureUnit,
                    LastPickDate = model.LastPickDate,
                    LastStoreDate = model.LastStoreDate,
                    LoadingUnitCode = model.LoadingUnitCode,
                    LoadingUnitHasCompartments = model.LoadingUnitHasCompartments,
                    LoadingUnitId = model.LoadingUnitId,
                    Lot = model.Lot,
                    MaterialStatusId = model.MaterialStatusId,
                    MaxCapacity = model.MaxCapacity,
                    PackageTypeId = model.PackageTypeId,
                    RegistrationNumber = model.RegistrationNumber,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    Stock = model.Stock,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    Width = model.Width,
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                });

                return new OperationResult<CompartmentDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<CompartmentDetails>(ex);
            }
        }

        #endregion
    }
}
