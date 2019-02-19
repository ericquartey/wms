using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService;

        private readonly IImageProvider imageProvider;

        private readonly IItemCategoryProvider itemCategoryProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly IMeasureUnitProvider measureUnitProvider;

        #endregion

        #region Constructors

        public ItemProvider(
            IImageProvider imageProvider,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService,
            IAbcClassProvider abcClassProvider,
            IItemCategoryProvider itemCategoryProvider,
            IMeasureUnitProvider measureUnitProvider)
        {
            this.itemsDataService = itemsDataService;
            this.compartmentsDataService = compartmentsDataService;
            this.imageProvider = imageProvider;
            this.abcClassProvider = abcClassProvider;
            this.itemCategoryProvider = itemCategoryProvider;
            this.measureUnitProvider = measureUnitProvider;
        }

        #endregion

        #region Methods

        public async Task AddEnumerationsAsync(ItemDetails itemDetails)
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
                    FifoTimeStore = model.FifoTimeStore,
                    Height = model.Height,
                    Image = model.Image,
                    InventoryDate = model.InventoryDate,
                    InventoryTolerance = model.InventoryTolerance,
                    ItemCategoryId = model.ItemCategoryId,
                    LastPickDate = model.LastPickDate,
                    LastStoreDate = model.LastStoreDate,
                    Length = model.Length,
                    ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                    MeasureUnitId = model.MeasureUnitId,
                    Note = model.Note,
                    PickTolerance = model.PickTolerance,
                    ReorderPoint = model.ReorderPoint,
                    ReorderQuantity = model.ReorderQuantity,
                    StoreTolerance = model.StoreTolerance,
                    Width = model.Width,
                    CompartmentsCount = model.CompartmentsCount
                });

                model.Id = item.Id;

                return new OperationResult<ItemDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemDetails>(ex);
            }
        }

        public Task<IOperationResult<ItemDetails>> DeleteAsync(int id) => throw new NotSupportedException();

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            string searchString = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            var items = await this.itemsDataService
                .GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchString);

            return items
                .Select(i => new Item
                {
                    Id = i.Id,
                    AbcClassDescription = i.AbcClassDescription,
                    AverageWeight = i.AverageWeight,
                    CreationDate = i.CreationDate,
                    FifoTimePick = i.FifoTimePick,
                    FifoTimeStore = i.FifoTimeStore,
                    Height = i.Height,
                    InventoryDate = i.InventoryDate,
                    InventoryTolerance = i.InventoryTolerance,
                    ManagementTypeDescription = i.ManagementType.ToString(), // TODO change
                    ItemCategoryDescription = i.ItemCategoryDescription,
                    LastModificationDate = i.LastModificationDate,
                    LastPickDate = i.LastPickDate,
                    LastStoreDate = i.LastStoreDate,
                    Length = i.Length,
                    MeasureUnitDescription = i.MeasureUnitDescription,
                    PickTolerance = i.PickTolerance,
                    ReorderPoint = i.ReorderPoint,
                    ReorderQuantity = i.ReorderQuantity,
                    StoreTolerance = i.StoreTolerance,
                    Width = i.Width,
                    Code = i.Code,
                    Description = i.Description,
                    TotalReservedForPick = i.TotalReservedForPick,
                    TotalReservedToStore = i.TotalReservedToStore,
                    TotalStock = i.TotalStock,
                    TotalAvailable = i.TotalAvailable,
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, string searchString = null)
        {
            return await this.itemsDataService.GetAllCountAsync(whereExpression?.ToString(), searchString);
        }

        public async Task<IEnumerable<AllowedItemInCompartment>> GetAllowedByCompartmentIdAsync(int compartmentId)
        {
            return (await this.compartmentsDataService.GetAllowedItemsAsync(compartmentId))
                .Select(ict => new AllowedItemInCompartment
                {
                    Id = ict.Id,
                    Code = ict.Code,
                    Description = ict.Description,
                    MaxCapacity = ict.MaxCapacity,
                    AbcClassDescription = ict.AbcClassDescription,
                    ItemCategoryDescription = ict.ItemCategoryDescription,
                    Image = ict.Image,
                });
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
        {
            var item = await this.itemsDataService.GetByIdAsync(id);

            var itemDetails = new ItemDetails
            {
                Id = item.Id,
                Code = item.Code,
                Description = item.Description,
                ItemCategoryId = item.ItemCategoryId,
                Note = item.Note,

                AbcClassId = item.AbcClassId,
                MeasureUnitId = item.MeasureUnitId,

                // TODO  MeasureUnitDescription = item.MeasureUnit.,
                ManagementType = (ItemManagementType)item.ManagementType,
                FifoTimePick = item.FifoTimePick,
                FifoTimeStore = item.FifoTimeStore,
                ReorderPoint = item.ReorderPoint,
                ReorderQuantity = item.ReorderQuantity,

                Height = item.Height,
                Length = item.Length,
                Width = item.Width,
                PickTolerance = item.PickTolerance,
                StoreTolerance = item.StoreTolerance,
                InventoryTolerance = item.InventoryTolerance,
                AverageWeight = item.AverageWeight,
                CompartmentsCount = item.CompartmentsCount,

                Image = item.Image,

                CreationDate = item.CreationDate,
                InventoryDate = item.InventoryDate,
                LastModificationDate = item.LastModificationDate,
                LastPickDate = item.LastPickDate,
                LastStoreDate = item.LastStoreDate,

                TotalAvailable = item.TotalAvailable
            };

            await this.AddEnumerationsAsync(itemDetails);

            return itemDetails;
        }

        public async Task<ItemDetails> GetNewAsync()
        {
            var itemDetails = new ItemDetails();
            itemDetails.ManagementType = ItemManagementType.FIFO;

            await this.AddEnumerationsAsync(itemDetails);

            return itemDetails;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(
                    Resources.Errors.ParameterCannotBeNullOrWhitespace, nameof(propertyName));
            }

            return await this.itemsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var originalItem = await this.itemsDataService.GetByIdAsync(model.Id);

                await this.itemsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.ItemDetails
                {
                    AbcClassId = model.AbcClassId,
                    AverageWeight = model.AverageWeight,
                    Code = model.Code,
                    Description = model.Description,
                    FifoTimePick = model.FifoTimePick,
                    FifoTimeStore = model.FifoTimeStore,
                    Height = model.Height,
                    Id = model.Id,
                    Image = model.Image,
                    InventoryDate = model.InventoryDate,
                    InventoryTolerance = model.InventoryTolerance,
                    ItemCategoryId = model.ItemCategoryId,
                    LastPickDate = model.LastPickDate,
                    LastStoreDate = model.LastStoreDate,
                    Length = model.Length,
                    ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                    MeasureUnitId = model.MeasureUnitId,
                    Note = model.Note,
                    PickTolerance = model.PickTolerance,
                    ReorderPoint = model.ReorderPoint,
                    ReorderQuantity = model.ReorderQuantity,
                    StoreTolerance = model.StoreTolerance,
                    Width = model.Width,
                    CompartmentsCount = model.CompartmentsCount
                });

                if (originalItem.Image != model.Image)
                {
                    this.SaveImage(model.ImagePath);
                }

                return new OperationResult<ItemDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemDetails>(ex);
            }
        }

        public async Task<IOperationResult<SchedulerRequest>> WithdrawAsync(ItemWithdraw itemWithdraw)
        {
            if (itemWithdraw == null)
            {
                throw new ArgumentNullException(nameof(itemWithdraw));
            }

            try
            {
                await this.itemsDataService.WithdrawAsync(
                   new WMS.Data.WebAPI.Contracts.SchedulerRequest
                   {
                       IsInstant = true,
                       Type = WMS.Data.WebAPI.Contracts.OperationType.Withdrawal,
                       ItemId = itemWithdraw.ItemDetails.Id,
                       BayId = itemWithdraw.BayId,
                       AreaId = itemWithdraw.AreaId.Value,
                       Lot = itemWithdraw.Lot,
                       RequestedQuantity = itemWithdraw.Quantity,
                       RegistrationNumber = itemWithdraw.RegistrationNumber,
                       Sub1 = itemWithdraw.Sub1,
                       Sub2 = itemWithdraw.Sub2,
                   });

                return new OperationResult<SchedulerRequest>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<SchedulerRequest>(ex);
            }
        }

        private void SaveImage(string imagePath)
        {
            this.imageProvider.SaveImage(imagePath);
        }

        #endregion
    }
}
