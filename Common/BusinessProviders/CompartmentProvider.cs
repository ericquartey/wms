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
            WMS.Data.WebAPI.Contracts.IAreasDataService areasDataService)
        {
            this.compartmentsDataService = compartmentsDataService;
            this.itemsDataService = itemsDataService;
            this.loadingUnitsDataService = loadingUnitsDataService;
            this.areasDataService = areasDataService;

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
                        XPosition = compartment.XPosition,
                        YPosition = compartment.YPosition,
                        LoadingUnitId = compartment.LoadingUnitId,
                        CompartmentTypeId = compartment.CompartmentTypeId,
                        IsItemPairingFixed = compartment.IsItemPairingFixed,
                        Stock = compartment.Stock,
                        ReservedForPick = compartment.ReservedForPick,
                        ReservedToStore = compartment.ReservedToStore,
                        CreationDate = DateTime.Now,
                        ItemId = compartment.ItemId,
                        MaterialStatusId = compartment.MaterialStatusId
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
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    CompartmentTypeId = model.CompartmentTypeId,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    Stock = model.Stock,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    CreationDate = DateTime.Now,
                    ItemId = model.ItemId,
                    MaterialStatusId = model.MaterialStatusId
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
                var compartment = await this.compartmentsDataService.GetByIdAsync(id);

                await this.compartmentsDataService.DeleteAsync(compartment);

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
                    Id = c.Id,
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    CompartmentTypeDescription = string.Format(
                       Resources.MasterData.CompartmentTypeListFormatReduced,
                       c.HasRotation ? c.Width : c.Height,
                       c.HasRotation ? c.Height : c.Width),
                    ItemDescription = c.ItemDescription,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    LoadingUnitCode = c.LoadingUnitCode,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatusDescription,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    ItemMeasureUnit = c.ItemMeasureUnit
                });
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
                Id = compartment.Id,
                LoadingUnitCode = compartment.LoadingUnitCode,
                CompartmentTypeId = compartment.CompartmentTypeId,
                IsItemPairingFixed = compartment.IsItemPairingFixed,
                ItemCode = compartment.ItemCode,
                ItemDescription = compartment.ItemDescription,
                Sub1 = compartment.Sub1,
                Sub2 = compartment.Sub2,
                MaterialStatusId = compartment.MaterialStatusId,
                FifoTime = compartment.FifoTime,
                PackageTypeId = compartment.PackageTypeId,
                Lot = compartment.Lot,
                RegistrationNumber = compartment.RegistrationNumber,
                MaxCapacity = compartment.MaxCapacity,
                Stock = compartment.Stock,
                ReservedForPick = compartment.ReservedForPick,
                ReservedToStore = compartment.ReservedToStore,
                CompartmentStatusId = compartment.CompartmentStatusId,
                CompartmentStatusDescription = compartment.CompartmentStatusDescription,
                CreationDate = compartment.CreationDate,
                LastHandlingDate = compartment.LastHandlingDate,
                InventoryDate = compartment.InventoryDate,
                FirstStoreDate = compartment.FirstStoreDate,
                LastStoreDate = compartment.LastStoreDate,
                LastPickDate = compartment.LastPickDate,
                Width = compartment.HasRotation ? compartment.Height : compartment.Width,
                Height = compartment.HasRotation ? compartment.Width : compartment.Height,
                XPosition = compartment.XPosition,
                YPosition = compartment.YPosition,
                LoadingUnitId = compartment.LoadingUnitId,
                ItemId = compartment.ItemId,
                LoadingUnitHasCompartments = compartment.LoadingUnitHasCompartments,
                ItemMeasureUnit = compartment.ItemMeasureUnit,
                LoadingUnit = loadingUnit,
                CompartmentStatusChoices = compartmentStatusChoices,
                CompartmentTypeChoices = compartmentTypeChoices,
                MaterialStatusChoices = materialStatusChoices,
                PackageTypeChoices = packageTypeChoices
            };
        }

        public async Task<IEnumerable<Compartment>> GetByItemIdAsync(int id)
        {
            return (await this.itemsDataService.GetCompartmentsAsync(id))
                .Select(c => new Compartment
                {
                    Id = c.Id,
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    ItemDescription = c.ItemDescription,
                    LoadingUnitCode = c.LoadingUnitCode,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatusDescription,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    IsItemPairingFixed = c.IsItemPairingFixed
                });
        }

        public async Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id)
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
                    Id = model.Id,
                    LoadingUnitCode = model.LoadingUnitCode,
                    CompartmentTypeId = model.CompartmentTypeId,
                    ItemCode = model.ItemCode,
                    ItemDescription = model.ItemDescription,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    MaterialStatusId = model.MaterialStatusId,
                    FifoTime = model.FifoTime,
                    PackageTypeId = model.PackageTypeId,
                    Lot = model.Lot,
                    RegistrationNumber = model.RegistrationNumber,
                    MaxCapacity = model.MaxCapacity,
                    Stock = model.Stock,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    CompartmentStatusId = model.CompartmentStatusId,
                    CompartmentStatusDescription = model.CompartmentStatusDescription,
                    CreationDate = model.CreationDate,
                    LastHandlingDate = model.LastHandlingDate,
                    InventoryDate = model.InventoryDate,
                    FirstStoreDate = model.FirstStoreDate,
                    LastStoreDate = model.LastStoreDate,
                    LastPickDate = model.LastPickDate,
                    Width = model.Width,
                    Height = model.Height,
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    ItemId = model.ItemId,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    LoadingUnitHasCompartments = model.LoadingUnitHasCompartments,
                    ItemMeasureUnit = model.ItemMeasureUnit
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
