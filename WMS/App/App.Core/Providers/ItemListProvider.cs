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
    public class ItemListProvider : IItemListProvider
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService;

        #endregion

        #region Constructors

        public ItemListProvider(
            IItemListRowProvider itemListRowProvider,
            WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService)
        {
            this.itemListRowProvider = itemListRowProvider;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var itemList = await this.itemListsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemListDetails
                {
                    AreaName = model.AreaName,
                    Code = model.Code,
                    CustomerOrderCode = model.CustomerOrderCode,
                    CustomerOrderDescription = model.CustomerOrderDescription,
                    Description = model.Description,
                    ItemListItemsCount = model.ItemListItemsCount,
                    ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                    ItemListTypeDescription = model.ItemListTypeDescription,
                    Job = model.Job,
                    Priority = model.Priority,
                    ShipmentUnitAssociated = model.ShipmentUnitAssociated,
                    ShipmentUnitCode = model.ShipmentUnitCode,
                    ShipmentUnitDescription = model.ShipmentUnitDescription,
                });

                model.Id = itemList.Id;

                return new OperationResult<ItemListDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemList>> ExecuteImmediatelyAsync(int listId, int areaId, int bayId)
        {
            try
            {
                await this.itemListsDataService.ExecuteAsync(
                    new WMS.Data.WebAPI.Contracts.ListExecutionRequest
                    {
                        ListId = listId,
                        AreaId = areaId,
                        BayId = bayId
                    });

                return new OperationResult<ItemList>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemList>(ex);
            }
        }

        public async Task<IEnumerable<ItemList>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var itemLists = await this.itemListsDataService
                .GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString);

            return itemLists
                .Select(l => new ItemList
                {
                    Id = l.Id,
                    Code = l.Code,
                    Description = l.Description,
                    Priority = l.Priority,
                    Status = (ItemListStatus)l.Status,
                    ItemListType = (ItemListType)l.ItemListType,
                    ItemListRowsCount = l.ItemListRowsCount,
                    ItemListItemsCount = l.ItemListRowsCount,
                    CreationDate = l.CreationDate
                });
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            return await this.itemListsDataService
                .GetAllCountAsync(whereString, searchString);
        }

        public async Task<ItemListDetails> GetByIdAsync(int id)
        {
            var itemList = await this.itemListsDataService.GetByIdAsync(id);

            var itemListStatusChoices = ((ItemListStatus[])Enum.GetValues(typeof(ItemListStatus)))
                .Select(i => new Enumeration((int)i, i.ToString())).ToList();

            var itemListRows = await this.itemListRowProvider.GetByItemListIdAsync(id);

            return new ItemListDetails
            {
                Id = itemList.Id,
                Code = itemList.Code,
                Description = itemList.Description,
                Priority = itemList.Priority,
                Status = (ItemListStatus)itemList.Status,
                ItemListType = (ItemListType)itemList.ItemListType,
                CreationDate = itemList.CreationDate,
                ItemListStatusChoices = itemListStatusChoices,
                ItemListRows = itemListRows,
                AreaName = itemList.AreaName,
                ItemListItemsCount = itemList.ItemListItemsCount,
                Job = itemList.Job,
                CustomerOrderCode = itemList.CustomerOrderCode,
                CustomerOrderDescription = itemList.CustomerOrderDescription,
                ShipmentUnitAssociated = itemList.ShipmentUnitAssociated,
                ShipmentUnitCode = itemList.ShipmentUnitCode,
                ShipmentUnitDescription = itemList.ShipmentUnitDescription,
                LastModificationDate = itemList.LastModificationDate,
                FirstExecutionDate = itemList.FirstExecutionDate,
                ExecutionEndDate = itemList.ExecutionEndDate,
                ItemListTypeDescription = itemList.ItemListTypeDescription,
                CanAddNewRow = itemList.CanAddNewRow,
                CanBeExecuted = itemList.CanBeExecuted
            };
        }

        public ItemListDetails GetNew()
        {
            return new ItemListDetails();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.itemListsDataService.GetUniqueValuesAsync(propertyName);
        }

        public async Task<IOperationResult<ItemList>> ScheduleForExecutionAsync(int listId, int areaId)
        {
            try
            {
                await this.itemListsDataService.ExecuteAsync(
                    new WMS.Data.WebAPI.Contracts.ListExecutionRequest
                    {
                        ListId = listId,
                        AreaId = areaId
                    });

                return new OperationResult<ItemList>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemList>(ex);
            }
        }

        public async Task<IOperationResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemListsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.ItemListDetails
                    {
                        Id = model.Id,
                        Code = model.Code,
                        Description = model.Description,
                        Priority = model.Priority,
                        ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                        CreationDate = model.CreationDate,
                        AreaName = model.AreaName,
                        ItemListItemsCount = model.ItemListItemsCount,
                        Job = model.Job,
                        CustomerOrderCode = model.CustomerOrderCode,
                        CustomerOrderDescription = model.CustomerOrderDescription,
                        ShipmentUnitAssociated = model.ShipmentUnitAssociated,
                        ShipmentUnitCode = model.ShipmentUnitCode,
                        ShipmentUnitDescription = model.ShipmentUnitDescription,
                        LastModificationDate = model.LastModificationDate,
                        FirstExecutionDate = model.FirstExecutionDate,
                        ExecutionEndDate = model.ExecutionEndDate,
                        CanAddNewRow = model.CanAddNewRow,
                        ItemListRowsCount = model.ItemListRowsCount,
                        ItemListTypeDescription = model.ItemListTypeDescription,
                        CanBeExecuted = model.CanBeExecuted
                    });

                return new OperationResult<ItemListDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListDetails>(ex);
            }
        }

        #endregion
    }
}
