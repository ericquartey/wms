using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.WPF;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "Ok")]
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly Data.WebAPI.Contracts.IAreasDataService areasDataService;

        private readonly Data.WebAPI.Contracts.ICellsDataService cellsDataService;

        private readonly Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService;

        private readonly ICompartmentStatusProvider compartmentStatusProvider;

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public CompartmentProvider(
            ICompartmentStatusProvider compartmentStatusProvider,
            ICompartmentTypeProvider compartmentTypeProvider,
            IPackageTypeProvider packageTypeProvider,
            IMaterialStatusProvider materialStatusProvider,
            ICellPositionProvider cellPositionProvider,
            ILoadingUnitProvider loadingUnitProvider,
            Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService,
            Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitsDataService,
            Data.WebAPI.Contracts.IAreasDataService areasDataService,
            Data.WebAPI.Contracts.ICellsDataService cellsDataService)
        {
            this.compartmentsDataService = compartmentsDataService;
            this.itemsDataService = itemsDataService;
            this.loadingUnitsDataService = loadingUnitsDataService;
            this.areasDataService = areasDataService;
            this.cellsDataService = cellsDataService;
            this.loadingUnitProvider = loadingUnitProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.compartmentStatusProvider = compartmentStatusProvider;
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<IDrawableCompartment>> AddRangeAsync(IEnumerable<IDrawableCompartment> compartments)
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
                        CompartmentTypeId = compartment.CompartmentTypeId.GetValueOrDefault(),
                        CreationDate = DateTime.Now,
                        Height = compartment.Height,
                        IsItemPairingFixed = compartment.IsItemPairingFixed,
                        ItemId = compartment.ItemId,
                        LoadingUnitId = compartment.LoadingUnitId.GetValueOrDefault(),
                        Lot = compartment.Lot,
                        MaterialStatusId = compartment.MaterialStatusId,
                        MaxCapacity = compartment.MaxCapacity,
                        PackageTypeId = compartment.PackageTypeId,
                        RegistrationNumber = compartment.RegistrationNumber,
                        ReservedForPick = compartment.ReservedForPick,
                        ReservedToPut = compartment.ReservedToPut,
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
                    CompartmentTypeId = model.CompartmentTypeId.GetValueOrDefault(),
                    CreationDate = DateTime.Now,
                    Height = model.Height,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    ItemId = model.ItemId,
                    LoadingUnitId = model.LoadingUnitId.GetValueOrDefault(),
                    Lot = model.Lot,
                    MaterialStatusId = model.MaterialStatusId,
                    MaxCapacity = model.MaxCapacity,
                    PackageTypeId = model.PackageTypeId,
                    RegistrationNumber = model.RegistrationNumber,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToPut = model.ReservedToPut,
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
                        Common.Resources.General.CompartmentTypeListFormatReduced,
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
                    Policies = c.GetPolicies(),
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
            return await this.compartmentsDataService.GetAllCountAsync(whereString, searchString);
        }

        public async Task<CompartmentDetails> GetByIdAsync(int id)
        {
            var compartment = await this.compartmentsDataService.GetByIdAsync(id);
            var compartmentStatusChoices = await this.compartmentStatusProvider.GetAllAsync();
            var compartmentTypeChoices = await this.compartmentTypeProvider.GetAllAsync();
            var materialStatusChoices = await this.materialStatusProvider.GetAllAsync();
            var packageTypeChoices = await this.packageTypeProvider.GetAllAsync();
            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(compartment.LoadingUnitId);

            return new CompartmentDetails
            {
                CompartmentStatusChoices = compartmentStatusChoices,
                CompartmentStatusDescription = compartment.CompartmentStatusDescription,
                CompartmentStatusId = compartment.CompartmentStatusId,
                CompartmentTypeChoices = compartmentTypeChoices,
                CompartmentTypeId = compartment.CompartmentTypeId,
                CreationDate = compartment.CreationDate,
                FifoStartDate = compartment.FifoStartDate,
                Height = compartment.HasRotation ? compartment.Width : compartment.Height,
                Id = compartment.Id,
                InventoryDate = compartment.InventoryDate,
                IsItemPairingFixed = compartment.IsItemPairingFixed,
                ItemCode = compartment.ItemCode,
                ItemDescription = compartment.ItemDescription,
                ItemId = compartment.ItemId,
                ItemMeasureUnit = compartment.ItemMeasureUnit,
                LastPickDate = compartment.LastPickDate,
                LastPutDate = compartment.LastPutDate,
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
                ReservedToPut = compartment.ReservedToPut,
                Stock = compartment.Stock,
                Sub1 = compartment.Sub1,
                Sub2 = compartment.Sub2,
                Width = compartment.HasRotation ? compartment.Height : compartment.Width,
                XPosition = compartment.XPosition,
                YPosition = compartment.YPosition,
                Policies = compartment.GetPolicies(),
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
                    Policies = c.GetPolicies(),
                });
        }

        public async Task<IOperationResult<IEnumerable<CompartmentDetails>>> GetByLoadingUnitIdAsync(int id)
        {
            try
            {
                var result = await this.loadingUnitsDataService.GetCompartmentsAsync(id);

                var compartments = result.Select(c => new CompartmentDetails
                {
                    CompartmentStatusDescription = c.CompartmentStatusDescription,
                    CompartmentStatusId = c.CompartmentStatusId,
                    CompartmentTypeId = c.CompartmentTypeId,
                    CreationDate = c.CreationDate,
                    FifoStartDate = c.FifoStartDate,
                    Height = c.HasRotation ? c.Width : c.Height,
                    Id = c.Id,
                    InventoryDate = c.InventoryDate,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemCode = c.ItemCode,
                    ItemDescription = c.ItemDescription,
                    ItemId = c.ItemId,
                    ItemMeasureUnit = c.ItemMeasureUnit,
                    LastPickDate = c.LastPickDate,
                    LastPutDate = c.LastPutDate,
                    LoadingUnitCode = c.LoadingUnitCode,
                    LoadingUnitHasCompartments = c.LoadingUnitHasCompartments,
                    LoadingUnitId = c.LoadingUnitId,
                    Lot = c.Lot,
                    MaterialStatusId = c.MaterialStatusId,
                    MaxCapacity = c.MaxCapacity,
                    PackageTypeId = c.PackageTypeId,
                    RegistrationNumber = c.RegistrationNumber,
                    ReservedForPick = c.ReservedForPick,
                    ReservedToPut = c.ReservedToPut,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    Width = c.HasRotation ? c.Height : c.Width,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                    Policies = c.GetPolicies(),
                });

                return new OperationResult<IEnumerable<CompartmentDetails>>(true, compartments);
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<CompartmentDetails>>(ex);
            }
        }

        public async Task<IEnumerable<Enumeration>> GetCellsByAreaIdAsync(int areaId)
        {
            return (await this.areasDataService.GetCellsAsync(areaId))
                .Select(c => new Enumeration(
                    c.Id,
                    $"{c.AreaName} - {c.AisleName} - Cell {c.Number} (Floor {c.Floor}, Column {c.Column}, {c.Side})")); // TODO: localize string
        }

        public async Task<double?> GetMaxCapacityAsync(double? width, double? height, int itemId)
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
                await this.compartmentsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.CompartmentDetails
                    {
                        CompartmentStatusDescription = model.CompartmentStatusDescription,
                        CompartmentStatusId = model.CompartmentStatusId,
                        CompartmentTypeId = model.CompartmentTypeId.GetValueOrDefault(),
                        CreationDate = model.CreationDate,
                        FifoStartDate = model.FifoStartDate,
                        Height = model.Height,
                        Id = model.Id,
                        InventoryDate = model.InventoryDate,
                        IsItemPairingFixed = model.IsItemPairingFixed,
                        ItemCode = model.ItemCode,
                        ItemDescription = model.ItemDescription,
                        ItemId = model.ItemId,
                        ItemMeasureUnit = model.ItemMeasureUnit,
                        LastPickDate = model.LastPickDate,
                        LastPutDate = model.LastPutDate,
                        LoadingUnitCode = model.LoadingUnitCode,
                        LoadingUnitHasCompartments = model.LoadingUnitHasCompartments,
                        LoadingUnitId = model.LoadingUnitId.GetValueOrDefault(),
                        Lot = model.Lot,
                        MaterialStatusId = model.MaterialStatusId,
                        MaxCapacity = model.MaxCapacity,
                        PackageTypeId = model.PackageTypeId,
                        RegistrationNumber = model.RegistrationNumber,
                        ReservedForPick = model.ReservedForPick,
                        ReservedToPut = model.ReservedToPut,
                        Stock = model.Stock,
                        Sub1 = model.Sub1,
                        Sub2 = model.Sub2,
                        Width = model.Width,
                        XPosition = model.XPosition,
                        YPosition = model.YPosition,
                    },
                    model.Id);

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
