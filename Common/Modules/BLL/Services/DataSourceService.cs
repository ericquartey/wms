using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Utils.Modules;
using Microsoft.Practices.ServiceLocation;
using Compartment = Ferretto.Common.BusinessModels.Compartment;

namespace Ferretto.Common.Modules.BLL.Services
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
                case MasterData.ITEMS:
                    return GetItemsDataSources<TModel, TKey>();

                case MasterData.COMPARTMENTS:
                    return GetCompartmentsDataSources<TModel, TKey>();

                case MasterData.CELLS:
                    return GetCellsDataSources<TModel, TKey>();

                case Machines.MACHINES:
                    return GetMachinesDataSources<TModel, TKey>();

                case MasterData.LOADINGUNITS:
                    return GetLoadingUnitsDataSources<TModel, TKey>();

                case MasterData.ITEMLISTS:
                    return GetItemListsDataSources<TModel, TKey>(parameter);

                case Scheduler.MISSIONS:
                    return GetMissionsDataSources<TModel, TKey>();

                case Scheduler.SCHEDULERREQUESTS:
                    return GetSchedulerRequestDataSources<TModel, TKey>();

                default:
                    return new List<IFilterDataSource<TModel, TKey>>();
            }
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetCellsDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
            var cellCountProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

            return new List<FilterDataSource<Cell, int>>
            {
                new FilterDataSource<Cell, int>(
                    "CellsViewAll",
                    Resources.MasterData.CellAll,
                    () => cellProvider.GetAll(),
                    () => cellCountProvider.GetAllCount()),

                new FilterDataSource<Cell, int>(
                    "CellStatusEmpty",
                    Resources.MasterData.CellStatusEmpty,
                    () => cellProvider.GetWithStatusEmpty(),
                    () => cellCountProvider.GetWithStatusEmptyCount()),
                new FilterDataSource<Cell, int>(
                    "CellStatusFull",
                    Resources.MasterData.CellStatusFull,
                    () => cellProvider.GetWithStatusFull(),
                    () => cellCountProvider.GetWithStatusFullCount()),
                new FilterDataSource<Cell, int>(
                    "CellClassA",
                    Resources.MasterData.CellClassA,
                    () => cellProvider.GetWithClassA(),
                    () => cellCountProvider.GetWithClassACount()),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetCompartmentsDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
            var compartmentCountProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

            return new List<FilterDataSource<Compartment, int>>
            {
                new FilterDataSource<Compartment, int>(
                    "CompartmentsViewAll",
                    Resources.MasterData.CompartmentAll,
                    () => compartmentProvider.GetAll(),
                    () => compartmentCountProvider.GetAllCount()),

                new FilterDataSource<Compartment, int>(
                    "CompartmentStatusAvailable",
                    Resources.MasterData.CompartmentStatusAvailable,
                    () => compartmentProvider.GetWithStatusAvailable(),
                    () => compartmentCountProvider.GetWithStatusAvailableCount()),

                new FilterDataSource<Compartment, int>(
                    "CompartmentStatusAwaiting",
                    Resources.MasterData.CompartmentStatusAwaiting,
                    () => compartmentProvider.GetWithStatusAwaiting(),
                    () => compartmentCountProvider.GetWithStatusAwaitingCount()),

                new FilterDataSource<Compartment, int>(
                    "CompartmentStatusExpired",
                    Resources.MasterData.CompartmentStatusExpired,
                    () => compartmentProvider.GetWithStatusExpired(),
                    () => compartmentCountProvider.GetWithStatusExpiredCount()),

                new FilterDataSource<Compartment, int>(
                    "CompartmentStatusBlocked",
                    Resources.MasterData.CompartmentStatusBlocked,
                    () => compartmentProvider.GetWithStatusBlocked(),
                    () => compartmentCountProvider.GetWithStatusBlockedCount()),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetItemListsDataSources<TModel, TKey>(object parameter)
            where TModel : IModel<TKey>
        {
            var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
            var itemListCountProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

            var type = parameter;
            if (parameter != null && Enum.IsDefined(typeof(ItemListType), (int)(char)parameter))
            {
                type = (ItemListType)Enum.ToObject(typeof(ItemListType), parameter);
            }

            var listFilters = new List<FilterDataSource<ItemList, int>>();
            switch (type)
            {
                case ItemListType.Pick:
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()));
                    break;

                case ItemListType.Put:
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            () => itemListProvider.GetWithTypePut(),
                            () => itemListCountProvider.GetWithTypePutCount()));
                    break;

                case ItemListType.Inventory:
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            () => itemListProvider.GetWithTypeInventory(),
                            () => itemListCountProvider.GetWithTypeInventoryCount()));
                    break;

                default:
                    listFilters.Add(new FilterDataSource<ItemList, int>(
                                        "ItemListViewAll",
                                        Resources.MasterData.ItemListAll,
                                        () => itemListProvider.GetAll(),
                                        () => itemListCountProvider.GetAllCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            () => itemListProvider.GetWithTypePut(),
                            () => itemListCountProvider.GetWithTypePutCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            () => itemListProvider.GetWithTypeInventory(),
                            () => itemListCountProvider.GetWithTypeInventoryCount()));
                    break;
            }

            listFilters.Add(
                new FilterDataSource<ItemList, int>(
                    "ItemListViewStatusWaiting",
                    Resources.MasterData.ItemListStatusWaiting,
                    () => itemListProvider.GetWithStatusWaiting((ItemListType?)type),
                    () => itemListCountProvider.GetWithStatusWaitingCount((ItemListType?)type)));

            listFilters.Add(
                new FilterDataSource<ItemList, int>(
                    "ItemListViewStatusCompleted",
                    Resources.MasterData.ItemListStatusCompleted,
                    () => itemListProvider.GetWithStatusCompleted((ItemListType?)type),
                    () => itemListCountProvider.GetWithStatusCompletedCount((ItemListType?)type)));

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
                    Resources.MasterData.ItemAll,
                    itemsProvider),

                new PagedDataSource<Item, int>(
                    "ItemsViewClassA",
                    Resources.MasterData.ItemClassA,
                    itemsProvider,
                    "[AbcClassDescription] == 'A Class'"),

                new PagedDataSource<Item, int>(
                    "ItemsViewFIFO",
                    Resources.MasterData.ItemFIFO,
                    itemsProvider,
                    "[ManagementType] == 'FIFO'")
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetLoadingUnitsDataSources<TModel, TKey>()
                    where TModel : IModel<TKey>
        {
            var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
            var loadingUnitCountProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

            return new List<FilterDataSource<LoadingUnit, int>>
            {
                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAll",
                    Resources.MasterData.LoadingUnitAll,
                    () => loadingUnitProvider.GetAll(),
                    () => loadingUnitCountProvider.GetAllCount()),

                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaManual",
                    Resources.MasterData.LoadingUnitAreaManual,
                    () => loadingUnitProvider.GetWithAreaManual(),
                    () => loadingUnitCountProvider.GetWithAreaManualCount()),
                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaVertimag",
                    Resources.MasterData.LoadingUnitAreaVertimag,
                    () => loadingUnitProvider.GetWithAreaVertimag(),
                    () => loadingUnitCountProvider.GetWithAreaVertimagCount()),

                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusAvailable",
                    Resources.MasterData.LoadingUnitStatusAvailable,
                    () => loadingUnitProvider.GetWithStatusAvailable(),
                    () => loadingUnitCountProvider.GetWithStatusAvailableCount()),
                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusBlocked",
                    Resources.MasterData.LoadingUnitStatusBlocked,
                    () => loadingUnitProvider.GetWithStatusBlocked(),
                    () => loadingUnitCountProvider.GetWithStatusBlockedCount()),
                new FilterDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusUsed",
                    Resources.MasterData.LoadingUnitStatusUsed,
                    () => loadingUnitProvider.GetWithStatusUsed(),
                    () => loadingUnitCountProvider.GetWithStatusUsedCount()),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetMachinesDataSources<TModel, TKey>()
                            where TModel : IModel<TKey>
        {
            var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();
            var machineCountProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

            return new List<FilterDataSource<Machine, int>>
            {
                new FilterDataSource<Machine, int>(
                    "MachinesViewAll",
                    Resources.Machines.MachineAll,
                    () => machineProvider.GetAll(),
                    () => machineCountProvider.GetAllCount()),

                new FilterDataSource<Machine, int>(
                    "MachinesViewVertimagXS",
                    Resources.Machines.MachineVertimagXS,
                    () => machineProvider.GetAllVertimagModelXs(),
                    () => machineCountProvider.GetAllVertimagModelXsCount()),

                new FilterDataSource<Machine, int>(
                    "MachinesViewVertimagM",
                    Resources.Machines.MachineVertimagM,
                    () => machineProvider.GetAllVertimagModelM(),
                    () => machineCountProvider.GetAllVertimagModelMCount())
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetMissionsDataSources<TModel, TKey>()
                    where TModel : IModel<TKey>
        {
            var missionProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();
            var missionCountProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();

            return new List<FilterDataSource<Mission, int>>
            {
                new FilterDataSource<Mission, int>(
                    "MissionViewAll",
                    Resources.Scheduler.MissionAll,
                    () => missionProvider.GetAll(),
                    () => missionCountProvider.GetAllCount()),

                new FilterDataSource<Mission, int>(
                    "MissionViewStatusCompleted",
                    Resources.Scheduler.MissionStatusCompleted,
                    () => missionProvider.GetWithStatusCompleted(),
                    () => missionCountProvider.GetWithStatusCompletedCount()),

                new FilterDataSource<Mission, int>(
                    "MissionViewStatusNew",
                    Resources.Scheduler.MissionStatusNew,
                    () => missionProvider.GetWithStatusNew(),
                    () => missionCountProvider.GetWithStatusNewCount())
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetSchedulerRequestDataSources<TModel, TKey>()
                                            where TModel : IModel<TKey>
        {
            var schedulerRequestProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();
            var schedulerRequestCountProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();

            return new List<FilterDataSource<SchedulerRequest, int>>
                    {
                        new FilterDataSource<SchedulerRequest, int>(
                            "SchedulerRequestViewAll",
                            Resources.Scheduler.SchedulerRequestViewAll,
                            () => schedulerRequestProvider.GetAll(),
                            () => schedulerRequestCountProvider.GetAllCount()),

                        new FilterDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationInsert",
                            Resources.Scheduler.SchedulerRequestOperationInsert,
                            () => schedulerRequestProvider.GetWithOperationTypeInsertion(),
                            () => schedulerRequestCountProvider.GetWithOperationTypeInsertionCount()),

                        new FilterDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationWithdraw",
                            Resources.Scheduler.SchedulerRequestOperationWithdraw,
                            () => schedulerRequestProvider.GetWithOperationTypeWithdrawal(),
                            () => schedulerRequestCountProvider.GetWithOperationTypeWithdrawalCount())
                    }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        #endregion
    }
}
