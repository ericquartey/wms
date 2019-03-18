using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Compartment = Ferretto.Common.BusinessModels.Compartment;

namespace Ferretto.WMS.App.Modules.BLL
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This method centralize the DataSource provision")]
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IFilterDataSource<TModel, TKey>> GetAllFilters<TModel, TKey>(string viewModelName, object parameter = null)
            where TModel : IModel<TKey>
        {
            switch (viewModelName)
            {
                case Common.Utils.Modules.MasterData.ITEMS:
                    return GetItemsDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.COMPARTMENTS:
                    return GetCompartmentsDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.CELLS:
                    return GetCellsDataSources<TModel, TKey>();

                case Common.Utils.Modules.Machines.MACHINES:
                    return GetMachinesDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.LOADINGUNITS:
                    return GetLoadingUnitsDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.ITEMLISTS:
                    return GetItemListsDataSources<TModel, TKey>(parameter);

                case Common.Utils.Modules.Scheduler.MISSIONS:
                    return GetMissionsDataSources<TModel, TKey>();

                case Common.Utils.Modules.Scheduler.SCHEDULERREQUESTS:
                    return GetSchedulerRequestDataSources<TModel, TKey>();

                default:
                    return new List<IFilterDataSource<TModel, TKey>>();
            }
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetCellsDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

            return new List<PagedDataSource<Cell, int>>
            {
                new PagedDataSource<Cell, int>(
                    "CellsViewAll",
                    Common.Resources.MasterData.CellAll,
                    cellProvider),

                 new PagedDataSource<Cell, int>(
                    "CellStatusEmpty",
                    Common.Resources.MasterData.CellStatusEmpty,
                    cellProvider,
                    "[Status] == 'Empty'"),

                 new PagedDataSource<Cell, int>(
                    "CellStatusFull",
                    Common.Resources.MasterData.CellStatusFull,
                    cellProvider,
                    "[Status] == 'Full'"),

                 new PagedDataSource<Cell, int>(
                    "CellClassA",
                    Common.Resources.MasterData.CellClassA,
                   cellProvider,
                    "[AbcClassDescription] == 'A Class'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetCompartmentsDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

            return new List<PagedDataSource<Compartment, int>>
            {
                new PagedDataSource<Compartment, int>(
                    "CompartmentsViewAll",
                    Common.Resources.MasterData.CompartmentAll,
                    compartmentProvider),

                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusAvailable",
                    Common.Resources.MasterData.CompartmentStatusAvailable,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Available'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusAwaiting",
                    Common.Resources.MasterData.CompartmentStatusAwaiting,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Awaiting verification'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusExpired",
                    Common.Resources.MasterData.CompartmentStatusExpired,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Expired'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusBlocked",
                    Common.Resources.MasterData.CompartmentStatusBlocked,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Blocked'"),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetItemListsDataSources<TModel, TKey>(object parameter)
            where TModel : IModel<TKey>
        {
            var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

            var type = parameter;
            if (parameter != null && Enum.IsDefined(typeof(ItemListType), (int)(char)parameter))
            {
                type = (ItemListType)Enum.ToObject(typeof(ItemListType), parameter);
            }

            var listFilters = new List<PagedDataSource<ItemList, int>>();
            switch (type)
            {
                case ItemListType.Pick:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Common.Resources.MasterData.ItemListsTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Pick}'"));
                    break;

                case ItemListType.Put:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Common.Resources.MasterData.ItemListsTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Put}'"));
                    break;

                case ItemListType.Inventory:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Common.Resources.MasterData.ItemListsTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Inventory}'"));
                    break;

                default:
                    listFilters.Add(new PagedDataSource<ItemList, int>(
                                        "ItemListViewAll",
                                        Common.Resources.MasterData.ItemListAll,
                                        itemListProvider));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Common.Resources.MasterData.ItemListsTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Pick}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Common.Resources.MasterData.ItemListsTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Put}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Common.Resources.MasterData.ItemListsTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Inventory}'"));
                    break;
            }

            var typeFilter = type != null ? $" && [ItemListType] == '{type}'" : string.Empty;

            listFilters.Add(
                new PagedDataSource<ItemList, int>(
                    "ItemListViewStatusWaiting",
                    Common.Resources.MasterData.ItemListStatusWaiting,
                    itemListProvider,
                    $"[ItemListStatus] == '{ItemListStatus.Waiting}' {typeFilter}"));

            listFilters.Add(
                new PagedDataSource<ItemList, int>(
                    "ItemListViewStatusCompleted",
                    Common.Resources.MasterData.ItemListStatusCompleted,
                    itemListProvider,
                    $"[ItemListStatus] == '{ItemListStatus.Completed}' {typeFilter}"));

            return listFilters.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetItemsDataSources<TModel, TKey>()
                                    where TModel : IModel<TKey>
        {
            var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
            return new List<PagedDataSource<Item, int>>
            {
                new PagedDataSource<Item, int>(
                    "ItemsViewAll",
                    Common.Resources.MasterData.ItemAll,
                    itemsProvider),

                new PagedDataSource<Item, int>(
                    "ItemsViewClassA",
                    Common.Resources.MasterData.ItemClassA,
                    itemsProvider,
                    "[AbcClassDescription] == 'A Class'"),

                new PagedDataSource<Item, int>(
                    "ItemsViewFIFO",
                    Common.Resources.MasterData.ItemFIFO,
                    itemsProvider,
                    $"[ManagementType] == '{ItemManagementType.FIFO}'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetLoadingUnitsDataSources<TModel, TKey>()
                    where TModel : IModel<TKey>
        {
            var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

            return new List<PagedDataSource<LoadingUnit, int>>
            {
                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAll",
                    Common.Resources.MasterData.LoadingUnitAll,
                    loadingUnitProvider),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaManual",
                    Common.Resources.MasterData.LoadingUnitAreaManual,
                    loadingUnitProvider,
                    $"[AreaName] == 'Manual Area'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaVertimag",
                    Common.Resources.MasterData.LoadingUnitAreaVertimag,
                    loadingUnitProvider,
                    "[AreaName] == 'Vertimag Area'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusAvailable",
                    Common.Resources.MasterData.LoadingUnitStatusAvailable,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Available'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusBlocked",
                    Common.Resources.MasterData.LoadingUnitStatusBlocked,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Blocked'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusUsed",
                    Common.Resources.MasterData.LoadingUnitStatusUsed,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Used'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetMachinesDataSources<TModel, TKey>()
                            where TModel : IModel<TKey>
        {
            var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

            return new List<PagedDataSource<Machine, int>>
            {
                new PagedDataSource<Machine, int>(
                    "MachinesViewAll",
                    Common.Resources.Machines.MachineAll,
                    machineProvider),

                // TODO: restore this when the Data WebAPI supports the 'Like' operator
                // new PagedDataSource<Machine>(
                //    "MachinesViewVertimagXS",
                //    Common.Resources.Machines.MachineVertimagXS,
                //    machineProvider,
                //    "UPPER([MachineTypeDescription]) == '%TRASLO%'"),

                // new PagedDataSource<Machine>(
                //    "MachinesViewVertimagXS",
                //    Common.Resources.Machines.MachineVertimagXS,
                //    machineProvider,
                //    "UPPER([MachineTypeDescription]) == '%VERTIMAG%'"),
                new PagedDataSource<Machine, int>(
                    "MachinesViewVertimagXS",
                    Common.Resources.Machines.MachineVertimagXS,
                    machineProvider,
                    "[Model] == 'VMAG/ver-2018/variant-XS/depth-103'"),

                new PagedDataSource<Machine, int>(
                    "MachinesViewVertimagM",
                    Common.Resources.Machines.MachineVertimagM,
                    machineProvider,
                    "[Model] == 'VMAG/ver-2018/variant-M/depth-84'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetMissionsDataSources<TModel, TKey>()
                    where TModel : IModel<TKey>
        {
            var missionProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();

            return new List<PagedDataSource<Mission, int>>
            {
                new PagedDataSource<Mission, int>(
                    "MissionViewAll",
                    Common.Resources.Scheduler.MissionAll,
                    missionProvider),

                new PagedDataSource<Mission, int>(
                    "MissionViewStatusCompleted",
                    Common.Resources.Scheduler.MissionStatusCompleted,
                    missionProvider,
                    $"[Status] == '{MissionStatus.Completed}'"),

                new PagedDataSource<Mission, int>(
                    "MissionViewStatusNew",
                    Common.Resources.Scheduler.MissionStatusNew,
                    missionProvider,
                    $"[Status] == '{MissionStatus.New}'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetSchedulerRequestDataSources<TModel, TKey>()
                                            where TModel : IModel<TKey>
        {
            var schedulerRequestProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();

            return new List<PagedDataSource<SchedulerRequest, int>>
                    {
                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestViewAll",
                            Common.Resources.Scheduler.SchedulerRequestViewAll,
                            schedulerRequestProvider),

                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationInsert",
                            Common.Resources.Scheduler.SchedulerRequestOperationInsert,
                            schedulerRequestProvider,
                            $"[OperationType] == '{OperationType.Insertion}'"),

                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationWithdraw",
                            Common.Resources.Scheduler.SchedulerRequestOperationWithdraw,
                            schedulerRequestProvider,
                            $"[OperationType] == '{OperationType.Withdrawal}'")
                    }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        #endregion
    }
}
