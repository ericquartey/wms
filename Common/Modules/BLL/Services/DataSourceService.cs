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

            return new List<PagedDataSource<Cell>>
            {
                new PagedDataSource<Cell>(
                    "CellsViewAll",
                    Resources.MasterData.CellAll,
                    cellProvider),

                 new PagedDataSource<Cell>(
                    "CellStatusEmpty",
                    Resources.MasterData.CellStatusEmpty,
                    cellProvider,
                    "[Status] == 'Empty'"),
                 new PagedDataSource<Cell>(
                    "CellStatusFull",
                    Resources.MasterData.CellStatusFull,
                    cellProvider,
                    "[Status] == 'Full'"),
                 new PagedDataSource<Cell>(
                    "CellClassA",
                    Resources.MasterData.CellClassA,
                   cellProvider,
                    "[AbcClassDescription] == 'A Class'")
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetCompartmentsDataSources<TModel>()
            where TModel : IBusinessObject
        {
            var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

            return new List<PagedDataSource<Compartment>>
            {
                new PagedDataSource<Compartment>(
                    "CompartmentsViewAll",
                    Resources.MasterData.CompartmentAll,
                    compartmentProvider),

                new PagedDataSource<Compartment>(
                    "CompartmentStatusAvailable",
                    Resources.MasterData.CompartmentStatusAvailable,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Available'"),
                new PagedDataSource<Compartment>(
                    "CompartmentStatusAwaiting",
                    Resources.MasterData.CompartmentStatusAwaiting,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Awaiting'"),
                new PagedDataSource<Compartment>(
                    "CompartmentStatusExpired",
                    Resources.MasterData.CompartmentStatusExpired,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Expired'"),
                new PagedDataSource<Compartment>(
                    "CompartmentStatusBlocked",
                    Resources.MasterData.CompartmentStatusBlocked,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Blocked'"),
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetItemListsDataSources<TModel>(object parameter)
            where TModel : IBusinessObject
        {
            var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

            var type = parameter;
            if (parameter != null && Enum.IsDefined(typeof(ItemListType), (int)(char)parameter))
            {
                type = (ItemListType)Enum.ToObject(typeof(ItemListType), parameter);
            }

            var listFilters = new List<PagedDataSource<ItemList>>();
            switch (type)
            {
                case ItemListType.Pick:
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Pick}'"));
                    break;

                case ItemListType.Put:
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Put}'"));
                    break;

                case ItemListType.Inventory:
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Inventory}'"));
                    break;

                default:
                    listFilters.Add(new PagedDataSource<ItemList>(
                                        "ItemListViewAll",
                                        Resources.MasterData.ItemListAll,
                                        itemListProvider));
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Pick}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Put}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{ItemListType.Inventory}'"));
                    break;
            }

            var typeFilter = type != null ? $" && [ItemListType] == '{type}'" : string.Empty;

            listFilters.Add(
                new PagedDataSource<ItemList>(
                    "ItemListViewStatusWaiting",
                    Resources.MasterData.ItemListStatusWaiting,
                    itemListProvider,
                    $"[ItemListStatus] == '{ItemListStatus.Waiting}' {typeFilter}"));

            listFilters.Add(
                new PagedDataSource<ItemList>(
                    "ItemListViewStatusCompleted",
                    Resources.MasterData.ItemListStatusCompleted,
                    itemListProvider,
                    $"[ItemListStatus] == '{ItemListStatus.Completed}' {typeFilter}"));

            return listFilters.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetItemsDataSources<TModel>()
                                    where TModel : IBusinessObject
        {
            var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
            return new List<PagedDataSource<Item>>
            {
                new PagedDataSource<Item>(
                    "ItemsViewAll",
                    Resources.MasterData.ItemAll,
                    itemsProvider),

                new PagedDataSource<Item>(
                    "ItemsViewClassA",
                    Resources.MasterData.ItemClassA,
                    itemsProvider,
                    "[AbcClassDescription] == 'A Class'"),

                new PagedDataSource<Item>(
                    "ItemsViewFIFO",
                    Resources.MasterData.ItemFIFO,
                    itemsProvider,
                    $"[ManagementType] == '{ItemManagementType.FIFO}'")
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetLoadingUnitsDataSources<TModel>()
                    where TModel : IBusinessObject
        {
            var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

            return new List<PagedDataSource<LoadingUnit>>
            {
                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewAll",
                    Resources.MasterData.LoadingUnitAll,
                    loadingUnitProvider),

                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewAreaManual",
                    Resources.MasterData.LoadingUnitAreaManual,
                    loadingUnitProvider,
                    $"[AreaName] == 'Manual Area'"),
                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewAreaVertimag",
                    Resources.MasterData.LoadingUnitAreaVertimag,
                    loadingUnitProvider,
                    "[AreaName] == 'Vertimag Area'"),

                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusAvailable",
                    Resources.MasterData.LoadingUnitStatusAvailable,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Available'"),
                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusBlocked",
                    Resources.MasterData.LoadingUnitStatusBlocked,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Blocked'"),
                new PagedDataSource<LoadingUnit>(
                    "LoadingUnitsViewStatusUsed",
                    Resources.MasterData.LoadingUnitStatusUsed,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Used'"),
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetMachinesDataSources<TModel>()
                            where TModel : IBusinessObject
        {
            var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

            return new List<PagedDataSource<Machine>>
            {
                new PagedDataSource<Machine>(
                    "MachinesViewAll",
                    Resources.Machines.MachineAll,
                    machineProvider),

                // new PagedDataSource<Machine>(
                //    "MachinesViewVertimagXS",
                //    Resources.Machines.MachineVertimagXS,
                //    machineProvider,
                //    "UPPER([MachineTypeDescription]) == '%TRASLO%'"),

                // new PagedDataSource<Machine>(
                //    "MachinesViewVertimagXS",
                //    Resources.Machines.MachineVertimagXS,
                //    machineProvider,
                //    "UPPER([MachineTypeDescription]) == '%VERTIMAG%'"),
                new PagedDataSource<Machine>(
                    "MachinesViewVertimagXS",
                    Resources.Machines.MachineVertimagXS,
                    machineProvider,
                    "[Model] == 'VMAG/ver-2018/variant-XS/depth-103'"),

                new PagedDataSource<Machine>(
                    "MachinesViewVertimagM",
                    Resources.Machines.MachineVertimagM,
                    machineProvider,
                    "[Model] == 'VMAG/ver-2018/variant-M/depth-84'")
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetMissionsDataSources<TModel>()
                    where TModel : IBusinessObject
        {
            var missionProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();

            return new List<PagedDataSource<Mission>>
            {
                new PagedDataSource<Mission>(
                    "MissionViewAll",
                    Resources.Scheduler.MissionAll,
                    missionProvider),

                new PagedDataSource<Mission>(
                    "MissionViewStatusCompleted",
                    Resources.Scheduler.MissionStatusCompleted,
                    missionProvider,
                    $"[Status] == '{MissionStatus.Completed}'"),

                new PagedDataSource<Mission>(
                    "MissionViewStatusNew",
                    Resources.Scheduler.MissionStatusNew,
                    missionProvider,
                    $"[Status] == '{MissionStatus.New}'")
            }.Cast<IFilterDataSource<TModel>>();
        }

        private static IEnumerable<IFilterDataSource<TModel>> GetSchedulerRequestDataSources<TModel>()
                                            where TModel : IBusinessObject
        {
            var schedulerRequestProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();

            return new List<PagedDataSource<SchedulerRequest>>
                    {
                        new PagedDataSource<SchedulerRequest>(
                            "SchedulerRequestViewAll",
                            Resources.Scheduler.SchedulerRequestViewAll,
                            schedulerRequestProvider),

                        new PagedDataSource<SchedulerRequest>(
                            "SchedulerRequestOperationInsert",
                            Resources.Scheduler.SchedulerRequestOperationInsert,
                            schedulerRequestProvider,
                            $"[OperationType] == '{OperationType.Insertion}'"),

                        new PagedDataSource<SchedulerRequest>(
                            "SchedulerRequestOperationWithdraw",
                            Resources.Scheduler.SchedulerRequestOperationWithdraw,
                            schedulerRequestProvider,
                            $"[OperationType] == '{OperationType.Withdrawal}'")
                    }.Cast<IFilterDataSource<TModel>>();
        }

        #endregion
    }
}
