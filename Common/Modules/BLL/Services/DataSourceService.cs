using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "CA1506",
            Justification = "This method centralize the DataSource provision")]
        public IEnumerable<IFilterDataSource<TModel>> GetAllFilters<TModel>(string viewModelName, object parameter = null)
            where TModel : IBusinessObject
        {
            switch (viewModelName)
            {
                case MasterData.ITEMS:
                    return GetItemsDataSources<TModel>();

                case MasterData.COMPARTMENTS:
                    return GetCompartmentsDataSources<TModel>();

                case MasterData.CELLS:
                    return GetCellsDataSources<TModel>();

                case Machines.MACHINES:
                    return GetMachinesDataSources<TModel>();

                case MasterData.LOADINGUNITS:
                    return GetLoadingUnitsDataSources<TModel>();

                case MasterData.ITEMLISTS:
                    return GetItemListsDataSources<TModel>(parameter);

                case Scheduler.MISSIONS:
                    return GetMissionsDataSources<TModel>();

                case Scheduler.SCHEDULERREQUESTS:
                    return GetSchedulerRequestDataSources<TModel>();

                default:
                    return new List<IFilterDataSource<TModel>>();
            }
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetCellsDataSources<TModel>()
            where TModel : IBusinessObject
        {
            var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
            var cellCountProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

            return new List<FilterDataSource<Cell>>
            {
                new FilterDataSource<Cell>(
                    "CellsViewAll",
                    Resources.MasterData.CellAll,
                    () => cellProvider.GetAll(),
                    () => cellCountProvider.GetAllCount()),

                new FilterDataSource<Cell>(
                    "CellStatusEmpty",
                    Resources.MasterData.CellStatusEmpty,
                    () => cellProvider.GetWithStatusEmpty(),
                    () => cellCountProvider.GetWithStatusEmptyCount()),
                new FilterDataSource<Cell>(
                    "CellStatusFull",
                    Resources.MasterData.CellStatusFull,
                    () => cellProvider.GetWithStatusFull(),
                    () => cellCountProvider.GetWithStatusFullCount()),
                new FilterDataSource<Cell>(
                    "CellClassA",
                    Resources.MasterData.CellClassA,
                    () => cellProvider.GetWithClassA(),
                    () => cellCountProvider.GetWithClassACount()),
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetCompartmentsDataSources<TModel>()
            where TModel : IBusinessObject
        {
            var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
            var compartmentCountProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

            return new List<FilterDataSource<Compartment>>
            {
                new FilterDataSource<Compartment>(
                    "CompartmentsViewAll",
                    Resources.MasterData.CompartmentAll,
                    () => compartmentProvider.GetAll(),
                    () => compartmentCountProvider.GetAllCount()),

                new FilterDataSource<Compartment>(
                    "CompartmentStatusAvailable",
                    Resources.MasterData.CompartmentStatusAvailable,
                    () => compartmentProvider.GetWithStatusAvailable(),
                    () => compartmentCountProvider.GetWithStatusAvailableCount()),
                new FilterDataSource<Compartment>(
                    "CompartmentStatusAwaiting",
                    Resources.MasterData.CompartmentStatusAwaiting,
                    () => compartmentProvider.GetWithStatusAwaiting(),
                    () => compartmentCountProvider.GetWithStatusAwaitingCount()),
                new FilterDataSource<Compartment>(
                    "CompartmentStatusExpired",
                    Resources.MasterData.CompartmentStatusExpired,
                    () => compartmentProvider.GetWithStatusExpired(),
                    () => compartmentCountProvider.GetWithStatusExpiredCount()),
                new FilterDataSource<Compartment>(
                    "CompartmentStatusBlocked",
                    Resources.MasterData.CompartmentStatusBlocked,
                    () => compartmentProvider.GetWithStatusBlocked(),
                    () => compartmentCountProvider.GetWithStatusBlockedCount()),
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetItemListsDataSources<TModel>(object parameter)
            where TModel : IBusinessObject
        {
            var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
            var itemListCountProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

            var type = parameter;
            if (parameter != null && Enum.IsDefined(typeof(ItemListType), (int)(char)parameter))
            {
                type = (ItemListType)Enum.ToObject(typeof(ItemListType), parameter);
            }

            var listFilters = new List<FilterDataSource<ItemList>>();
            switch (type)
            {
                case ItemListType.Pick:
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()));
                    break;

                case ItemListType.Put:
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            () => itemListProvider.GetWithTypePut(),
                            () => itemListCountProvider.GetWithTypePutCount()));
                    break;

                case ItemListType.Inventory:
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            () => itemListProvider.GetWithTypeInventory(),
                            () => itemListCountProvider.GetWithTypeInventoryCount()));
                    break;

                default:
                    listFilters.Add(new FilterDataSource<ItemList>(
                                        "ItemListViewAll",
                                        Resources.MasterData.ItemListAll,
                                        () => itemListProvider.GetAll(),
                                        () => itemListCountProvider.GetAllCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            () => itemListProvider.GetWithTypePut(),
                            () => itemListCountProvider.GetWithTypePutCount()));
                    listFilters.Add(
                        new FilterDataSource<ItemList>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            () => itemListProvider.GetWithTypeInventory(),
                            () => itemListCountProvider.GetWithTypeInventoryCount()));
                    break;
            }

            listFilters.Add(
                new FilterDataSource<ItemList>(
                    "ItemListViewStatusWaiting",
                    Resources.MasterData.ItemListStatusWaiting,
                    () => itemListProvider.GetWithStatusWaiting((ItemListType?)type),
                    () => itemListCountProvider.GetWithStatusWaitingCount((ItemListType?)type)));

            listFilters.Add(
                new FilterDataSource<ItemList>(
                    "ItemListViewStatusCompleted",
                    Resources.MasterData.ItemListStatusCompleted,
                    () => itemListProvider.GetWithStatusCompleted((ItemListType?)type),
                    () => itemListCountProvider.GetWithStatusCompletedCount((ItemListType?)type)));

            return listFilters.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetItemsDataSources<TModel>()
                                    where TModel : IBusinessObject
        {
            var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
            var itemsCountProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
            return new List<FilterDataSource<Item>>
            {
                new FilterDataSource<Item>(
                    "ItemsViewAll",
                    Resources.MasterData.ItemAll,
                    () => itemsProvider.GetAll(),
                    () => itemsCountProvider.GetAllCount()),

                new FilterDataSource<Item>(
                    "ItemsViewClassA",
                    Resources.MasterData.ItemClassA,
                    () => itemsProvider.GetWithAClass(),
                    () => itemsCountProvider.GetWithAClassCount()),

                new FilterDataSource<Item>(
                    "ItemsViewFIFO",
                    Resources.MasterData.ItemFIFO,
                    () => itemsProvider.GetWithFifo(),
                    () => itemsCountProvider.GetWithFifoCount())
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetLoadingUnitsDataSources<TModel>()
                    where TModel : IBusinessObject
        {
            var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
            var loadingUnitCountProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

            return new List<FilterDataSource<LoadingUnit>>
            {
                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewAll",
                    Resources.MasterData.LoadingUnitAll,
                    () => loadingUnitProvider.GetAll(),
                    () => loadingUnitCountProvider.GetAllCount()),

                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewAreaManual",
                    Resources.MasterData.LoadingUnitAreaManual,
                    () => loadingUnitProvider.GetWithAreaManual(),
                    () => loadingUnitCountProvider.GetWithAreaManualCount()),
                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewAreaVertimag",
                    Resources.MasterData.LoadingUnitAreaVertimag,
                    () => loadingUnitProvider.GetWithAreaVertimag(),
                    () => loadingUnitCountProvider.GetWithAreaVertimagCount()),

                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusAvailable",
                    Resources.MasterData.LoadingUnitStatusAvailable,
                    () => loadingUnitProvider.GetWithStatusAvailable(),
                    () => loadingUnitCountProvider.GetWithStatusAvailableCount()),
                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusBlocked",
                    Resources.MasterData.LoadingUnitStatusBlocked,
                    () => loadingUnitProvider.GetWithStatusBlocked(),
                    () => loadingUnitCountProvider.GetWithStatusBlockedCount()),
                new FilterDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusUsed",
                    Resources.MasterData.LoadingUnitStatusUsed,
                    () => loadingUnitProvider.GetWithStatusUsed(),
                    () => loadingUnitCountProvider.GetWithStatusUsedCount()),
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetMachinesDataSources<TModel>()
                            where TModel : IBusinessObject
        {
            var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();
            var machineCountProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

            return new List<FilterDataSource<Machine>>
            {
                new FilterDataSource<Machine>(
                    "MachinesViewAll",
                    Resources.Machines.MachineAll,
                    () => machineProvider.GetAll(),
                    () => machineCountProvider.GetAllCount()),

                new FilterDataSource<Machine>(
                    "MachinesViewVertimagXS",
                    Resources.Machines.MachineVertimagXS,
                    () => machineProvider.GetAllVertimagModelXS(),
                    () => machineCountProvider.GetAllVertimagModelXSCount()),

                new FilterDataSource<Machine>(
                    "MachinesViewVertimagM",
                    Resources.Machines.MachineVertimagM,
                    () => machineProvider.GetAllVertimagModelM(),
                    () => machineCountProvider.GetAllVertimagModelMCount())
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetMissionsDataSources<TModel>()
                    where TModel : IBusinessObject
        {
            var missionProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();
            var missionCountProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();

            return new List<FilterDataSource<Mission>>
            {
                new FilterDataSource<Mission>(
                    "MissionViewAll",
                    Resources.Scheduler.MissionAll,
                    () => missionProvider.GetAll(),
                    () => missionCountProvider.GetAllCount()),

                new FilterDataSource<Mission>(
                    "MissionViewStatusCompleted",
                    Resources.Scheduler.MissionStatusCompleted,
                    () => missionProvider.GetWithStatusCompleted(),
                    () => missionCountProvider.GetWithStatusCompletedCount()),

                new FilterDataSource<Mission>(
                    "MissionViewStatusNew",
                    Resources.Scheduler.MissionStatusNew,
                    () => missionProvider.GetWithStatusNew(),
                    () => missionCountProvider.GetWithStatusNewCount())
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetSchedulerRequestDataSources<TModel>()
                                            where TModel : IBusinessObject
        {
            var schedulerRequestProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();
            var schedulerRequestCountProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();

            return new List<FilterDataSource<SchedulerRequest>>
                    {
                        new FilterDataSource<SchedulerRequest>(
                            "SchedulerRequestViewAll",
                            Resources.Scheduler.SchedulerRequestViewAll,
                            () => schedulerRequestProvider.GetAll(),
                            () => schedulerRequestCountProvider.GetAllCount()),

                        new FilterDataSource<SchedulerRequest>(
                            "SchedulerRequestOperationInsert",
                            Resources.Scheduler.SchedulerRequestOperationInsert,
                            () => schedulerRequestProvider.GetWithOperationTypeInsertion(),
                            () => schedulerRequestCountProvider.GetWithOperationTypeInsertionCount()),

                        new FilterDataSource<SchedulerRequest>(
                            "SchedulerRequestOperationWithdraw",
                            Resources.Scheduler.SchedulerRequestOperationWithdraw,
                            () => schedulerRequestProvider.GetWithOperationTypeWithdrawal(),
                            () => schedulerRequestCountProvider.GetWithOperationTypeWithdrawalCount())
                    }.Cast<IFilterDataSource<TModel>>();
        }

        #endregion Methods
    }
}
