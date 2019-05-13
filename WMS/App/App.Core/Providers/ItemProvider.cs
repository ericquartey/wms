using System;
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
    public class ItemProvider : IItemProvider
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService;

        private readonly IItemCategoryProvider itemCategoryProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService;

        private readonly IMeasureUnitProvider measureUnitProvider;

        #endregion

        #region Constructors

        public ItemProvider(
            WMS.Data.WebAPI.Contracts.IItemsDataService itemsDataService,
            WMS.Data.WebAPI.Contracts.ICompartmentsDataService compartmentsDataService,
            IAbcClassProvider abcClassProvider,
            IItemCategoryProvider itemCategoryProvider,
            IMeasureUnitProvider measureUnitProvider)
        {
            this.itemsDataService = itemsDataService;
            this.compartmentsDataService = compartmentsDataService;
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
                    FifoTimePut = model.FifoTimePut,
                    Height = model.Height,
                    Image = model.Image,
                    InventoryDate = model.InventoryDate,
                    InventoryTolerance = model.InventoryTolerance,
                    ItemCategoryId = model.ItemCategoryId,
                    LastPickDate = model.LastPickDate,
                    LastPutDate = model.LastPutDate,
                    Length = model.Length,
                    ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                    MeasureUnitId = model.MeasureUnitId,
                    Note = model.Note,
                    PickTolerance = model.PickTolerance,
                    ReorderPoint = model.ReorderPoint,
                    ReorderQuantity = model.ReorderQuantity,
                    PutTolerance = model.PutTolerance,
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

        public async Task<IEnumerable<Item>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
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
                    Length = i.Length,
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

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.itemsDataService.GetAllCountAsync(whereString, searchString);
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
                }).ToList();
        }

        public async Task<ItemDetails> GetByIdAsync(int id)
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
                Length = item.Length,
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

        public async Task<ItemDetails> GetNewAsync()
        {
            var itemDetails = new ItemDetails();

            await this.AddEnumerationsAsync(itemDetails);

            return itemDetails;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(
                    Common.Resources.Errors.ParameterCannotBeNullOrWhitespace, nameof(propertyName));
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
                        Length = model.Length,
                        ManagementType = (WMS.Data.WebAPI.Contracts.ItemManagementType)model.ManagementType,
                        MeasureUnitDescription = model.MeasureUnitDescription,
                        MeasureUnitId = model.MeasureUnitId,
                        Note = model.Note,
                        PickTolerance = model.PickTolerance,
                        ReorderPoint = model.ReorderPoint,
                        ReorderQuantity = model.ReorderQuantity,
                        PutTolerance = model.PutTolerance,
                        Width = model.Width,
                    },
                    model.Id);

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
                    itemWithdraw.ItemDetails.Id,
                    new Data.WebAPI.Contracts.ItemWithdrawOptions
                    {
                        AreaId = itemWithdraw.AreaId.GetValueOrDefault(),
                        BayId = itemWithdraw.BayId,
                        RunImmediately = true,
                        Lot = itemWithdraw.Lot,
                        RegistrationNumber = itemWithdraw.RegistrationNumber,
                        RequestedQuantity = itemWithdraw.Quantity.GetValueOrDefault(),
                        Sub1 = itemWithdraw.Sub1,
                        Sub2 = itemWithdraw.Sub2
                    });

                return new OperationResult<SchedulerRequest>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<SchedulerRequest>(ex);
            }
        }

        #endregion
    }
}
