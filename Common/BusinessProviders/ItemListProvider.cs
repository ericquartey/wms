using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemListProvider : IItemListProvider
    {
        #region Fields

        private readonly ItemListRowProvider itemListRowProvider;

        private readonly WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService;

        private readonly IItemListsSchedulerService itemListsSchedulerService;

        #endregion

        #region Constructors

        public ItemListProvider(
            ItemListRowProvider itemListRowProvider,
            IItemListsSchedulerService itemListsSchedulerService,
            WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService)
        {
            this.itemListRowProvider = itemListRowProvider;
            this.itemListsSchedulerService = itemListsSchedulerService;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult> CreateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var itemList = await this.itemListsDataService.CreateAsync(new WMS.Data.WebAPI.Contracts.ItemListDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    Description = model.Description,
                    Priority = model.Priority,
                    ItemListStatus = (WMS.Data.WebAPI.Contracts.ItemListStatus)model.ItemListStatus,
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

                model.Id = itemList.Id;

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public async Task<OperationResult> ExecuteImmediatelyAsync(int listId, int areaId, int bayId)
        {
            try
            {
                await this.itemListsSchedulerService.ExecuteAsync(new WMS.Scheduler.WebAPI.Contracts.ListExecutionRequest { ListId = listId, AreaId = areaId, BayId = bayId });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public async Task<IEnumerable<ItemList>> GetAllAsync(
            int skip = 0,
            int take = 0,
            IEnumerable<SortOption> orderBy = null,
            IExpression whereExpression = null,
            IExpression searchExpression = null)
        {
            var orderByString = orderBy != null ? string.Join(",", orderBy.Select(s => $"{s.PropertyName} {s.Direction}")) : null;

            return (await this.itemListsDataService.GetAllAsync(skip, take, whereExpression?.ToString(), orderByString, searchExpression?.ToString()))
                .Select(l => new ItemList
                {
                    Id = l.Id,
                    Code = l.Code,
                    Description = l.Description,
                    Priority = l.Priority,
                    ItemListStatus = (ItemListStatus)l.ItemListStatus,
                    ItemListType = (ItemListType)l.ItemListType,
                    ItemListRowsCount = l.ItemListRowsCount,
                    ItemListItemsCount = l.ItemListRowsCount,
                    CreationDate = l.CreationDate
                });
        }

        public async Task<int> GetAllCountAsync(IExpression whereExpression = null, IExpression searchExpression = null)
        {
            return await this.itemListsDataService.GetAllCountAsync(whereExpression?.ToString(), searchExpression?.ToString());
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
                ItemListStatus = (ItemListStatus)itemList.ItemListStatus,
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

        public async Task<OperationResult> ScheduleForExecutionAsync(int listId, int areaId)
        {
            try
            {
                await this.itemListsSchedulerService.ExecuteAsync(
                    new ListExecutionRequest
                    {
                        ListId = listId,
                        AreaId = areaId
                    });

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public async Task<IOperationResult> UpdateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemListsDataService.UpdateAsync(new WMS.Data.WebAPI.Contracts.ItemListDetails
                {
                    Id = model.Id,
                    Code = model.Code,
                    Description = model.Description,
                    Priority = model.Priority,
                    ItemListStatus = (WMS.Data.WebAPI.Contracts.ItemListStatus)model.ItemListStatus,
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

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        #endregion
    }
}
