using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Extensions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.App.Core.Providers
{
    public class ItemListRowProvider : IItemListRowProvider
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService;

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly IPackageTypeProvider packageTypeProvider;

        #endregion

        #region Constructors

        public ItemListRowProvider(
            IMaterialStatusProvider materialStatusProvider,
            IPackageTypeProvider packageTypeProvider,
            WMS.Data.WebAPI.Contracts.IItemListRowsDataService itemListRowsDataService,
            WMS.Data.WebAPI.Contracts.IItemListsDataService itemListsDataService)
        {
            this.packageTypeProvider = packageTypeProvider;
            this.materialStatusProvider = materialStatusProvider;
            this.itemListRowsDataService = itemListRowsDataService;
            this.itemListsDataService = itemListsDataService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var itemListRow = await this.itemListRowsDataService.CreateAsync(new Data.WebAPI.Contracts.ItemListRowDetails
                {
                    Code = model.Code,
                    Priority = model.Priority,
                    ItemId = model.ItemId.GetValueOrDefault(),
                    ItemListId = model.ItemListId,
                    RequestedQuantity = model.RequestedQuantity.GetValueOrDefault(),
                    DispatchedQuantity = model.DispatchedQuantity.GetValueOrDefault(),
                    Status = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.Status,
                    ItemListCode = model.ItemListCode,
                    Lot = model.Lot,
                    RegistrationNumber = model.RegistrationNumber,
                    Sub1 = model.Sub1,
                    Sub2 = model.Sub2,
                    PackageTypeId = model.PackageTypeId,
                    MaterialStatusId = model.MaterialStatusId,
                });

                model.Id = itemListRow.Id;

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRowDetails>> DeleteAsync(int id)
        {
            try
            {
                await this.itemListRowsDataService.DeleteAsync(id);

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRow>> ExecuteImmediatelyAsync(int listRowId, int areaId, int bayId)
        {
            try
            {
                await this.itemListRowsDataService.ExecuteAsync(listRowId, areaId, bayId);

                return new OperationResult<ItemListRow>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRow>(ex);
            }
        }

        public async Task<IEnumerable<ItemListRow>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            try
            {
                return (await this.itemListRowsDataService.GetAllAsync(skip, take, whereString, orderBySortOptions.ToQueryString(), searchString))
                    .Select(l => new ItemListRow
                    {
                        Id = l.Id,
                        Code = l.Code,
                        DispatchedQuantity = l.DispatchedQuantity,
                        ItemDescription = l.ItemDescription,
                        Status = (Enums.ItemListRowStatus)l.Status,
                        ItemUnitMeasure = l.ItemUnitMeasure,
                        MaterialStatusDescription = l.MaterialStatusDescription,
                        RequestedQuantity = l.RequestedQuantity,
                        Priority = l.Priority,
                        CreationDate = l.CreationDate,
                        Policies = l.GetPolicies(),
                    });
            }
            catch
            {
                return new List<ItemListRow>();
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.itemListRowsDataService.GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<ItemListRowDetails> GetByIdAsync(int id)
        {
            try
            {
                var row = await this.itemListRowsDataService.GetByIdAsync(id);

                var enumeration = new ItemListRowDetails();
                await this.AddEnumerationsAsync(enumeration);

                return new ItemListRowDetails
                {
                    Id = row.Id,
                    Code = row.Code,
                    Priority = row.Priority,
                    ItemId = row.ItemId,
                    ItemImage = row.ItemImage,
                    RequestedQuantity = row.RequestedQuantity,
                    DispatchedQuantity = row.DispatchedQuantity,
                    Status = (Enums.ItemListRowStatus)row.Status,
                    ItemDescription = row.ItemDescription,
                    CreationDate = row.CreationDate,
                    ItemListCode = row.ItemListCode,
                    ItemListDescription = row.ItemListDescription,
                    ItemListType = (Enums.ItemListType)row.ItemListType,
                    CompletionDate = row.CompletionDate,
                    LastExecutionDate = row.LastExecutionDate,
                    LastModificationDate = row.LastModificationDate,
                    Lot = row.Lot,
                    RegistrationNumber = row.RegistrationNumber,
                    Sub1 = row.Sub1,
                    Sub2 = row.Sub2,
                    PackageTypeId = row.PackageTypeId,
                    MaterialStatusId = row.MaterialStatusId,
                    ItemUnitMeasure = row.ItemUnitMeasure,
                    MaterialStatusChoices = enumeration.MaterialStatusChoices,
                    PackageTypeChoices = enumeration.PackageTypeChoices,
                    ItemListId = row.ItemListId,
                    Policies = row.GetPolicies(),
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IOperationResult<IEnumerable<ItemListRow>>> GetByItemListIdAsync(int id)
        {
            try
            {
                var result = (await this.itemListsDataService.GetRowsAsync(id))
                    .Select(r => new ItemListRow
                    {
                        Id = r.Id,
                        Code = r.Code,
                        Priority = r.Priority,
                        ItemDescription = r.ItemDescription,
                        RequestedQuantity = r.RequestedQuantity,
                        DispatchedQuantity = r.DispatchedQuantity,
                        Status = (Enums.ItemListRowStatus)r.Status,
                        MaterialStatusDescription = r.MaterialStatusDescription,
                        CreationDate = r.CreationDate,
                        ItemUnitMeasure = r.ItemUnitMeasure,
                        Policies = r.GetPolicies(),
                    });

                return new OperationResult<IEnumerable<ItemListRow>>(true, result);
            }
            catch (Exception e)
            {
                return new OperationResult<IEnumerable<ItemListRow>>(e);
            }
        }

        public async Task<IOperationResult<ItemListRowDetails>> GetNewAsync(int idList)
        {
            try
            {
                var row = new ItemListRowDetails();
                row.ItemListId = idList;
                await this.AddEnumerationsAsync(row);
                return new OperationResult<ItemListRowDetails>(true, row);
            }
            catch (Exception e)
            {
                return new OperationResult<ItemListRowDetails>(e);
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return await this.itemListRowsDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IOperationResult<ItemListRow>> ScheduleForExecutionAsync(int listRowId, int areaId)
        {
            try
            {
                await this.itemListRowsDataService.ExecuteAsync(listRowId, areaId, bayId: null);

                return new OperationResult<ItemListRow>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRow>(ex);
            }
        }

        public async Task<IOperationResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                await this.itemListRowsDataService.UpdateAsync(
                    new WMS.Data.WebAPI.Contracts.ItemListRowDetails
                    {
                        Id = model.Id,
                        Code = model.Code,
                        Priority = model.Priority,
                        ItemId = model.ItemId.GetValueOrDefault(),
                        RequestedQuantity = model.RequestedQuantity.GetValueOrDefault(),
                        DispatchedQuantity = model.DispatchedQuantity.GetValueOrDefault(),
                        Status = (WMS.Data.WebAPI.Contracts.ItemListRowStatus)model.Status,
                        ItemDescription = model.ItemDescription,
                        CreationDate = model.CreationDate,
                        ItemListCode = model.ItemListCode,
                        ItemListDescription = model.ItemListDescription,
                        ItemListType = (WMS.Data.WebAPI.Contracts.ItemListType)model.ItemListType,
                        CompletionDate = model.CompletionDate,
                        LastExecutionDate = model.LastExecutionDate,
                        LastModificationDate = model.LastModificationDate,
                        Lot = model.Lot,
                        RegistrationNumber = model.RegistrationNumber,
                        Sub1 = model.Sub1,
                        Sub2 = model.Sub2,
                        PackageTypeId = model.PackageTypeId,
                        MaterialStatusId = model.MaterialStatusId,
                        ItemUnitMeasure = model.ItemUnitMeasure,
                        ItemListId = model.ItemListId,
                    },
                    model.Id);

                return new OperationResult<ItemListRowDetails>(true);
            }
            catch (Exception ex)
            {
                return new OperationResult<ItemListRowDetails>(ex);
            }
        }

        private async Task AddEnumerationsAsync(ItemListRowDetails row)
        {
            if (row != null)
            {
                row.MaterialStatusChoices = await this.materialStatusProvider.GetAllAsync();
                row.PackageTypeChoices = await this.packageTypeProvider.GetAllAsync();
            }
        }

        #endregion
    }
}
