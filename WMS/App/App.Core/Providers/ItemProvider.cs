using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Providers
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService;

        private readonly Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService;

        private readonly IItemCategoryProvider itemCategoryProvider;

        private readonly Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitDataService;

        private readonly IMeasureUnitProvider measureUnitProvider;

        #endregion

        #region Constructors

        public ItemProvider(
            Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService,
            Data.WebAPI.Contracts.ILoadingUnitsDataService loadingUnitDataService,
            Data.WebAPI.Contracts.ICompartmentTypesDataService compartmentTypesDataService,
            IAbcClassProvider abcClassProvider,
            IItemCategoryProvider itemCategoryProvider,
            IMeasureUnitProvider measureUnitProvider)
        {
            this.itemsDataService = itemsDataService;
            this.compartmentsDataService = compartmentsDataService;
            this.loadingUnitDataService = loadingUnitDataService;
            this.compartmentTypesDataService = compartmentTypesDataService;
            this.abcClassProvider = abcClassProvider;
            this.itemCategoryProvider = itemCategoryProvider;
            this.measureUnitProvider = measureUnitProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var item = await this.itemsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemDetails
                {
                    AbcClassId = model.AbcClassId,
                    AverageWeight = model.AverageWeight,
                    Code = model.Code,
                    Description = model.Description,
                    FifoTimePick = model.FifoTimePick,
                    FifoTimePut = model.FifoTimePut,
                    Height = model.Height,
                    Image = model.Image,
                    InventoryDate = model.InventoryDate,
                    InventoryTolerance = model.InventoryTolerance,
                    ItemCategoryId = model.ItemCategoryId,
                    LastPickDate = model.LastPickDate,
                    LastPutDate = model.LastPutDate,
                    Depth = model.Depth,
                    ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                    MeasureUnitId = model.MeasureUnitId,
                    Note = model.Note,
                    PickTolerance = model.PickTolerance,
                    ReorderPoint = model.ReorderPoint,
                    ReorderQuantity = model.ReorderQuantity,
                    PutTolerance = model.PutTolerance,
                    Width = model.Width,
                    CompartmentsCount = model.CompartmentsCount,
                    UploadImageData = model.ImagePath != null ? File.ReadAllBytes(model.ImagePath) : null,
                    UploadImageName = Path.GetFileName(model.ImagePath),
                });

                model.Id = item.Id;

                return new OperationResult<ItemDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemCompartmentType>> CreateCompartmentTypeAssociationAsync(
            int itemId,
            int compartmentTypeId,
            int maxCapacity)
        {
            try
            {
                var result = await this.itemsDataService
                    .AddCompartmentTypeAssociationAsync(itemId, compartmentTypeId, maxCapacity);

                var itemCompartmentType = new ItemCompartmentType
                {
                    CompartmentTypeId = result.CompartmentTypeId,
                    ItemId = result.ItemId,
                    MaxCapacity = result.MaxCapacity,
                    Policies = result.GetPolicies(),
                };

                return new OperationResult<ItemCompartmentType>(true, itemCompartmentType);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemCompartmentType>(ex);
            }
        }

        public async Task<IOperationResult<ItemDetails>> DeleteAsync(int id)
        {
            try
            {
                await this.itemsDataService.DeleteAsync(id);

                return new OperationResult<ItemDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemCompartmentType>> DeleteCompartmentTypeAssociationAsync(
            int itemId,
            int compartmentTypeId)
        {
            try
            {
                var result = await this.itemsDataService
                    .DeleteCompartmentTypeAssociationAsync(itemId, compartmentTypeId);

                var itemCompartmentType = new ItemCompartmentType
                {
                    CompartmentTypeId = result.CompartmentTypeId,
                    ItemId = result.ItemId,
                    MaxCapacity = result.MaxCapacity,
                };

                return new OperationResult<ItemCompartmentType>(true, itemCompartmentType);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemCompartmentType>(ex);
            }
        }

        public async Task<IOperationResult<IEnumerable<Item>>> GetAllAllowedByCompartmentTypeIdAsync(
               int compartmentTypeId)
        {
            var result = await this.compartmentTypesDataService.GetAllAllowedItemsByCompartmentTypeAsync(compartmentTypeId);
            var items = result.Select(i => new Item
            {
                Id = i.Id,
                Code = i.Code,
                Description = i.Description,
                Image = i.Image,
            }).ToList();

            return new OperationResult<IEnumerable<Item>>(true, items);
        }

        public async Task<IOperationResult<IEnumerable<Item>>> GetAllAllowedByLoadingUnitIdAsync(
                                                int loadingUnitId,
                                                int skip,
                                                int take,
                                                IEnumerable<SortOption> orderBySortOptions = null)
        {
            try
            {
                var items = await this.loadingUnitDataService
                    .GetAllAllowedByLoadingUnitIdAsync(loadingUnitId, skip, take, orderBySortOptions.ToQueryString());

                var result = items
                    .Select(i => new Item
                    {
                        Id = i.Id,
                        AbcClassDescription = i.AbcClassDescription,
                        AverageWeight = i.AverageWeight,
                        CreationDate = i.CreationDate,
                        FifoTimePick = i.FifoTimePick,
                        FifoTimePut = i.FifoTimePut,
                        Height = i.Height,
                        Image = i.Image,
                        InventoryDate = i.InventoryDate,
                        InventoryTolerance = i.InventoryTolerance,
                        ManagementTypeDescription = i.ManagementType.ToString(), // TODO change
                        ItemCategoryDescription = i.ItemCategoryDescription,
                        LastModificationDate = i.LastModificationDate,
                        LastPickDate = i.LastPickDate,
                        LastPutDate = i.LastPutDate,
                        Depth = i.Depth,
                        MeasureUnitDescription = i.MeasureUnitDescription,
                        PickTolerance = i.PickTolerance,
                        ReorderPoint = i.ReorderPoint,
                        ReorderQuantity = i.ReorderQuantity,
                        PutTolerance = i.PutTolerance,
                        Width = i.Width,
                        Code = i.Code,
                        Description = i.Description,
                        TotalReservedForPick = i.TotalReservedForPick,
                        TotalReservedToPut = i.TotalReservedToPut,
                        TotalStock = i.TotalStock,
                        TotalAvailable = i.TotalAvailable,
                        Policies = i.GetPolicies(),
                    });

                return new OperationResult<IEnumerable<Item>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<Item>>(e);
            }
        }

        public async Task<IOperationResult<int>> GetAllAllowedByLoadingUnitIdCountAsync(int loadingUnitId)
        {
            try
            {
                var result = await this.loadingUnitDataService.GetAllAllowedByLoadingUnitIdCountAsync(loadingUnitId);
                return new OperationResult<int>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<int>(e);
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemWithCompartmentTypeInfo>>> GetAllAssociatedByCompartmentTypeIdAsync(
           int compartmentTypeId)
        {
            try
            {
                var items = await this.compartmentTypesDataService.GetAllAssociatedItemWithCompartmentTypeAsync(compartmentTypeId);
                var result = items.Select(i => new ItemWithCompartmentTypeInfo
                {
                    Id = i.Id,
                    AbcClassDescription = i.AbcClassDescription,
                    Code = i.Code,
                    Description = i.Description,
                    ItemCategoryDescription = i.ItemCategoryDescription,
                    MaxCapacity = i.MaxCapacity,
                    MeasureUnitDescription = i.MeasureUnitDescription,
                    TotalAvailable = i.TotalAvailable,
                    TotalReservedForPick = i.TotalReservedForPick,
                    TotalReservedToPut = i.TotalReservedToPut,
                    TotalStock = i.TotalStock,
                    Policies = i.GetPolicies(),
                });

                return new OperationResult<IEnumerable<ItemWithCompartmentTypeInfo>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<ItemWithCompartmentTypeInfo>>(e);
            }
        }

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            try
            {
                var items = await this.itemsDataService
                    .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

                return items
                    .Select(i => new Item
                    {
                        Id = i.Id,
                        AbcClassDescription = i.AbcClassDescription,
                        AverageWeight = i.AverageWeight,
                        CreationDate = i.CreationDate,
                        FifoTimePick = i.FifoTimePick,
                        FifoTimePut = i.FifoTimePut,
                        Height = i.Height,
                        Image = i.Image,
                        InventoryDate = i.InventoryDate,
                        InventoryTolerance = i.InventoryTolerance,
                        ManagementTypeDescription = i.ManagementType.ToString(), // TODO change
                        ItemCategoryDescription = i.ItemCategoryDescription,
                        LastModificationDate = i.LastModificationDate,
                        LastPickDate = i.LastPickDate,
                        LastPutDate = i.LastPutDate,
                        Depth = i.Depth,
                        MeasureUnitDescription = i.MeasureUnitDescription,
                        PickTolerance = i.PickTolerance,
                        ReorderPoint = i.ReorderPoint,
                        ReorderQuantity = i.ReorderQuantity,
                        PutTolerance = i.PutTolerance,
                        Width = i.Width,
                        Code = i.Code,
                        Description = i.Description,
                        TotalReservedForPick = i.TotalReservedForPick,
                        TotalReservedToPut = i.TotalReservedToPut,
                        TotalStock = i.TotalStock,
                        TotalAvailable = i.TotalAvailable,
                        Policies = i.GetPolicies(),
                    });
            }
            catch
            {
                return new List<Item>();
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemCompartmentType>>>
            GetAllCompartmentTypeAssociationsAsync(int itemId)
        {
            try
            {
                var result = await this.itemsDataService
                    .GetAllCompartmentTypeAssociationsByIdAsync(itemId);

                var itemCompartmentTypes = result.Select(ict => new ItemCompartmentType
                {
                    CompartmentTypeId = ict.CompartmentTypeId,
                    ItemId = ict.ItemId,
                    MaxCapacity = ict.MaxCapacity,
                    Policies = ict.GetPolicies(),
                });

                return new OperationResult<IEnumerable<ItemCompartmentType>>(true, itemCompartmentTypes);
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<ItemCompartmentType>>(ex);
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.itemsDataService.GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<IOperationResult<IEnumerable<AllowedItemInCompartment>>> GetAllowedByCompartmentIdAsync(int compartmentId)
        {
            try
            {
                var result = (await this.compartmentsDataService.GetAllowedItemsAsync(compartmentId))
                    .Select(ict => new AllowedItemInCompartment
                    {
                        Id = ict.Id,
                        Code = ict.Code,
                        Description = ict.Description,
                        MaxCapacity = ict.MaxCapacity,
                        AbcClassDescription = ict.AbcClassDescription,
                        ItemCategoryDescription = ict.ItemCategoryDescription,
                        Image = ict.Image,
                    }).ToList();

                return new OperationResult<IEnumerable<AllowedItemInCompartment>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<AllowedItemInCompartment>>(e);
            }
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            try
            {
                var item = await this.itemsDataService.GetByIdAsync(id);

                var itemDetails = new ItemDetails
                {
                    AbcClassId = item.AbcClassId,
                    AverageWeight = item.AverageWeight,
                    Code = item.Code,
                    CompartmentsCount = item.CompartmentsCount,
                    CreationDate = item.CreationDate,
                    Description = item.Description,
                    FifoTimePick = item.FifoTimePick,
                    FifoTimePut = item.FifoTimePut,
                    Height = item.Height,
                    Id = item.Id,
                    Image = item.Image,
                    InventoryDate = item.InventoryDate,
                    InventoryTolerance = item.InventoryTolerance,
                    ItemCategoryId = item.ItemCategoryId,
                    LastModificationDate = item.LastModificationDate,
                    LastPickDate = item.LastPickDate,
                    LastPutDate = item.LastPutDate,
                    Depth = item.Depth,
                    ManagementType = (ItemManagementType)item.ManagementType,
                    MeasureUnitDescription = item.MeasureUnitDescription,
                    MeasureUnitId = item.MeasureUnitId,
                    Note = item.Note,
                    PickTolerance = item.PickTolerance,
                    ReorderPoint = item.ReorderPoint,
                    ReorderQuantity = item.ReorderQuantity,
                    PutTolerance = item.PutTolerance,
                    TotalAvailable = item.TotalAvailable,
                    Width = item.Width,
                    Policies = item.GetPolicies(),
                };

                await this.AddEnumerationsAsync(itemDetails);

                return itemDetails;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IOperationResult<ItemDetails>> GetNewAsync()
        {
            try
            {
                var itemDetails = new ItemDetails();
                await this.AddEnumerationsAsync(itemDetails);
                return new OperationResult<ItemDetails>(true, itemDetails);
            }
            catch (Exception e)
            {
                return new OperationResult<ItemDetails>(e);
            }
        }

        public async Task<IOperationResult<double>> GetPickAvailabilityAsync(
                    ItemPick itemPick,
                    CancellationToken cancellationToken = default(CancellationToken))
        {
            if (itemPick == null)
            {
                throw new ArgumentNullException(nameof(itemPick));
            }

            try
            {
                var availability = await this.itemsDataService.GetPickAvailabilityAsync(
                    itemPick.ItemDetails.Id,
                    SelectItemOptions(itemPick),
                    cancellationToken);

                return new OperationResult<double>(true, availability);
            }
            catch (Exception ex)
            {
                return new OperationResult<double>(ex);
            }
        }

        public async Task<IOperationResult<double>> GetPutCapacityAsync(
            ItemPut itemPut,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (itemPut == null)
            {
                throw new ArgumentNullException(nameof(itemPut));
            }

            try
            {
                var capacity = await this.itemsDataService.GetPutCapacityAsync(
                    itemPut.ItemDetails.Id,
                    this.SelectItemOptions(itemPut),
                    cancellationToken);

                return new OperationResult<double>(true, capacity);
            }
            catch (Exception ex)
            {
                return new OperationResult<double>(ex);
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new ArgumentException(
                        Errors.ParameterCannotBeNullOrWhitespace, nameof(propertyName));
                }

                return await this.itemsDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return new List<object>();
            }
        }

        public async Task<IOperationResult<SchedulerRequest>> PickAsync(ItemPick itemPick)
        {
            if (itemPick == null)
            {
                throw new ArgumentNullException(nameof(itemPick));
            }

            try
            {
                await this.itemsDataService.PickAsync(
                    itemPick.ItemDetails.Id,
                    SelectItemOptions(itemPick));

                return new OperationResult<SchedulerRequest>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<SchedulerRequest>(ex);
            }
        }

        public async Task<IOperationResult<SchedulerRequest>> PutAsync(ItemPut itemPut)
        {
            if (itemPut == null)
            {
                throw new ArgumentNullException(nameof(itemPut));
            }

            try
            {
                await this.itemsDataService.PutAsync(
                    itemPut.ItemDetails.Id,
                    this.SelectItemOptions(itemPut));

                return new OperationResult<SchedulerRequest>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<SchedulerRequest>(ex);
            }
        }

        public async Task<IOperationResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.ItemDetails
                    {
                        AbcClassId = model.AbcClassId,
                        AverageWeight = model.AverageWeight,
                        Code = model.Code,
                        CompartmentsCount = model.CompartmentsCount,
                        Description = model.Description,
                        FifoTimePick = model.FifoTimePick,
                        FifoTimePut = model.FifoTimePut,
                        Height = model.Height,
                        Id = model.Id,
                        Image = model.Image,
                        InventoryDate = model.InventoryDate,
                        InventoryTolerance = model.InventoryTolerance,
                        ItemCategoryId = model.ItemCategoryId,
                        LastPickDate = model.LastPickDate,
                        LastPutDate = model.LastPutDate,
                        Depth = model.Depth,
                        ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                        MeasureUnitDescription = model.MeasureUnitDescription,
                        MeasureUnitId = model.MeasureUnitId,
                        Note = model.Note,
                        PickTolerance = model.PickTolerance,
                        ReorderPoint = model.ReorderPoint,
                        ReorderQuantity = model.ReorderQuantity,
                        PutTolerance = model.PutTolerance,
                        Width = model.Width,
                        UploadImageData = model.ImagePath != null ? File.ReadAllBytes(model.ImagePath) : null,
                        UploadImageName = Path.GetFileName(model.ImagePath),
                    },
                    model.Id);

                return new OperationResult<ItemDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemCompartmentType>> UpdateCompartmentTypeAssociationAsync(
            int itemId,
            int compartmentTypeId,
            int maxCapacity)
        {
            try
            {
                var result = await this.itemsDataService
                    .UpdateCompartmentTypeAssociationAsync(itemId, compartmentTypeId, maxCapacity);

                var itemCompartmentType = new ItemCompartmentType
                {
                    CompartmentTypeId = result.CompartmentTypeId,
                    ItemId = result.ItemId,
                    MaxCapacity = result.MaxCapacity,
                    Policies = result.GetPolicies(),
                };

                return new OperationResult<ItemCompartmentType>(true, itemCompartmentType);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemCompartmentType>(ex);
            }
        }

        private static Data.WebAPI.Contracts.ItemOptions SelectItemOptions(ItemPick itemPick)
        {
            return new Data.WebAPI.Contracts.ItemOptions
            {
                AreaId = itemPick.AreaId.GetValueOrDefault(),
                BayId = itemPick.BayId,
                RunImmediately = true,
                Lot = itemPick.Lot,
                RegistrationNumber = itemPick.RegistrationNumber,
                RequestedQuantity = itemPick.Quantity.GetValueOrDefault(),
                MaterialStatusId = itemPick.MaterialStatusId,
                PackageTypeId = itemPick.PackageTypeId,
                Sub1 = itemPick.Sub1,
                Sub2 = itemPick.Sub2,
            };
        }

        private async Task AddEnumerationsAsync(ItemDetails itemDetails)
        {
            if (itemDetails != null)
            {
                itemDetails.AbcClassChoices = await this.abcClassProvider.GetAllAsync();
                itemDetails.MeasureUnitChoices = await this.measureUnitProvider.GetAllAsync();
                itemDetails.ManagementTypeChoices = ((ItemManagementType[])Enum.GetValues(typeof(ItemManagementType)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();
                itemDetails.ItemCategoryChoices = await this.itemCategoryProvider.GetAllAsync();
            }
        }

        private Data.WebAPI.Contracts.ItemOptions SelectItemOptions(ItemPut itemPut)
        {
            return new Data.WebAPI.Contracts.ItemOptions
            {
                AreaId = itemPut.AreaId.GetValueOrDefault(),
                BayId = itemPut.BayId,
                MaterialStatusId = itemPut.MaterialStatusId,
                RunImmediately = true,
                Lot = itemPut.Lot,
                PackageTypeId = itemPut.PackageTypeId,
                RegistrationNumber = itemPut.RegistrationNumber,
                RequestedQuantity = itemPut.Quantity.GetValueOrDefault(),
                Sub1 = itemPut.Sub1,
                Sub2 = itemPut.Sub2,
            };
        }

        #endregion
    }
}
