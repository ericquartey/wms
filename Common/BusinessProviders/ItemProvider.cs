using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly IDatabaseContextService dataContextService;

        private readonly IImageProvider imageProvider;

        private readonly IItemCategoryProvider itemCategoryProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly WMS.Scheduler.WebAPI.Contracts.IItemsSchedulerService itemsSchedulerService;

        private readonly IMeasureUnitProvider measureUnitProvider;

        #endregion

        #region Constructors

        public ItemProvider(
            IDatabaseContextService dataContextService,
            IImageProvider imageProvider,
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Scheduler.WebAPI.Contracts.IItemsSchedulerService itemsSchedulerService,
            IAbcClassProvider abcClassProvider,
            IItemCategoryProvider itemCategoryProvider,
            IMeasureUnitProvider measureUnitProvider)
        {
            this.dataContextService = dataContextService;
            this.itemsSchedulerService = itemsSchedulerService;
            this.itemsDataService = itemsDataService;
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
                itemDetails.ManagementTypeChoices = EnumerationProvider.GetAllItemManagementTypes();
                itemDetails.ItemCategoryChoices = await this.itemCategoryProvider.GetAllAsync();
            }
        }

        public async Task<IOperationResult> CreateAsync(ItemDetails model)
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
                    Width = model.Width
                });

                model.Id = item.Id;

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.itemsDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
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

        public int GetAllCount()
        {
            using (var dc = this.dataContextService.Current)
            {
                return dc.Items.AsNoTracking().Count();
            }
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.itemsDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
        }

        public IQueryable<AllowedItemInCompartment> GetAllowedByCompartmentId(int compartmentId)
        {
            return this.dataContextService.Current.Compartments
                .Where(c => c.Id == compartmentId)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.AbcClass)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.ItemCategory)
                .SelectMany(
                    c => c.CompartmentType.ItemsCompartmentTypes,
                    (c, ict) => new AllowedItemInCompartment
                    {
                        Id = ict.Item.Id,
                        Code = ict.Item.Code,
                        Description = ict.Item.Description,
                        MaxCapacity = ict.MaxCapacity,
                        AbcClassDescription = ict.Item.AbcClass.Description,
                        ItemCategoryDescription = ict.Item.ItemCategory.Description,
                        Image = ict.Item.Image,
                    })
                .AsNoTracking();
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

        public bool HasAnyCompartments(int itemId)
        {
            using (var dc = this.dataContextService.Current)
            {
                return dc.Compartments.AsNoTracking().Any(c => c.ItemId == itemId);
            }
        }

        public async Task<IOperationResult> UpdateAsync(ItemDetails model)
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
                    Width = model.Width
                });

                if (originalItem.Image != model.Image)
                {
                    this.SaveImage(model.ImagePath);
                }

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public async Task<IOperationResult> WithdrawAsync(ItemWithdraw itemWithdraw)
        {
            if (itemWithdraw == null)
            {
                throw new ArgumentNullException(nameof(itemWithdraw));
            }

            try
            {
                await this.itemsSchedulerService.WithdrawAsync(
                   new WMS.Scheduler.WebAPI.Contracts.SchedulerRequest
                   {
                       IsInstant = true,
                       Type = WMS.Scheduler.WebAPI.Contracts.OperationType.Withdrawal,
                       ItemId = itemWithdraw.ItemDetails.Id,
                       BayId = itemWithdraw.BayId,
                       AreaId = itemWithdraw.AreaId.Value,
                       Lot = itemWithdraw.Lot,
                       RequestedQuantity = itemWithdraw.Quantity,
                       RegistrationNumber = itemWithdraw.RegistrationNumber,
                       Sub1 = itemWithdraw.Sub1,
                       Sub2 = itemWithdraw.Sub2,
                   });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        private void SaveImage(string imagePath)
        {
            this.imageProvider.SaveImage(imagePath);
        }

        #endregion
    }
}
