using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IDataSource<TModel, TKey>> GetAllFilters<TModel, TKey>(string viewModelName, object parameter = null)
            where TModel : IModel<TKey>
        {
            switch (viewModelName)
            {
                case Common.Utils.Modules.MasterData.ITEMS:
                    return GetItemsDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.COMPARTMENTS:
                    return GetCompartmentsDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.COMPARTMENTTYPES:
                    return GetCompartmentTypesDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.CELLS:
                    return GetCellsDataSources<TModel, TKey>();

                case Common.Utils.Modules.Machines.MACHINES:
                    return GetMachinesDataSources<TModel, TKey>();

                case Common.Utils.Modules.MasterData.LOADINGUNITS:
                    return GetLoadingUnitsDataSources<TModel, TKey>();

                case Common.Utils.Modules.ItemLists.ITEMLISTS:
                    return GetItemListsDataSources<TModel, TKey>(parameter);

                case Common.Utils.Modules.Scheduler.MISSIONS:
                    return GetMissionsDataSources<TModel, TKey>();

                case Common.Utils.Modules.Scheduler.SCHEDULERREQUESTS:
                    return GetSchedulerRequestDataSources<TModel, TKey>();

                default:
                    return new List<IDataSource<TModel, TKey>>();
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
                    Resources.MasterData.CellAll,
                    cellProvider),

                new PagedDataSource<Cell, int>(
                    "CellStatusEmpty",
                    Resources.MasterData.CellStatusEmpty,
                    cellProvider,
                    "[Status] == 'Empty'"),

                new PagedDataSource<Cell, int>(
                    "CellStatusFull",
                    Resources.MasterData.CellStatusFull,
                    cellProvider,
                    "[Status] == 'Full'"),

                new PagedDataSource<Cell, int>(
                    "CellClassA",
                    Resources.MasterData.CellClassA,
                    cellProvider,
                    "[AbcClassDescription] == 'A Class'"),
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
                    Resources.MasterData.CompartmentAll,
                    compartmentProvider),

                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusAvailable",
                    Resources.MasterData.CompartmentStatusAvailable,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Available'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusAwaiting",
                    Resources.MasterData.CompartmentStatusAwaiting,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Awaiting verification'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusExpired",
                    Resources.MasterData.CompartmentStatusExpired,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Expired'"),
                new PagedDataSource<Compartment, int>(
                    "CompartmentStatusBlocked",
                    Resources.MasterData.CompartmentStatusBlocked,
                    compartmentProvider,
                    "[MaterialStatusDescription] == 'Blocked'"),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetCompartmentTypesDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var compartmentTypeProvider = ServiceLocator.Current.GetInstance<ICompartmentTypeProvider>();

            return new List<PagedDataSource<CompartmentType, int>>
            {
                new PagedDataSource<CompartmentType, int>(
                    "CompartmentTypesViewAll",
                    Resources.MasterData.CompartmentTypeAll,
                    compartmentTypeProvider),
                new PagedDataSource<CompartmentType, int>(
                    "CompartmentTypeNotUsedType",
                    Resources.MasterData.CompartmentTypeNotUsedType,
                    compartmentTypeProvider,
                    "[CompartmentsCount] == 0"),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetItemListsDataSources<TModel, TKey>(object parameter)
            where TModel : IModel<TKey>
        {
            var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

            var type = parameter;
            if (parameter != null && Enum.IsDefined(typeof(Enums.ItemListType), (int)(char)parameter))
            {
                type = (Enums.ItemListType)Enum.ToObject(typeof(Enums.ItemListType), parameter);
            }

            var listFilters = new List<PagedDataSource<ItemList, int>>();
            switch (type)
            {
                case Enums.ItemListType.Pick:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsAllTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Pick}'"));
                    break;

                case Enums.ItemListType.Put:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsAllTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Put}'"));
                    break;

                case Enums.ItemListType.Inventory:
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsAllTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Inventory}'"));
                    break;

                default:
                    listFilters.Add(new PagedDataSource<ItemList, int>(
                                        "ItemListViewAll",
                                        Resources.MasterData.ItemListAll,
                                        itemListProvider));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Pick}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypePut",
                            Resources.MasterData.ItemListsTypePut,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Put}'"));
                    listFilters.Add(
                        new PagedDataSource<ItemList, int>(
                            "ItemListViewTypeInventory",
                            Resources.MasterData.ItemListsTypeInventory,
                            itemListProvider,
                            $"[ItemListType] == '{Enums.ItemListType.Inventory}'"));
                    break;
            }

            var typeFilter = type != null ? $" && [ItemListType] == '{type}'" : string.Empty;

            listFilters.Add(
                new PagedDataSource<ItemList, int>(
                    "ItemListViewStatusWaiting",
                    Resources.MasterData.ItemListStatusWaiting,
                    itemListProvider,
                    $"[Status] == '{Enums.ItemListStatus.Waiting}' {typeFilter}"));

            listFilters.Add(
                new PagedDataSource<ItemList, int>(
                    "ItemListViewStatusCompleted",
                    Resources.MasterData.ItemListStatusCompleted,
                    itemListProvider,
                    $"[Status] == '{Enums.ItemListStatus.Completed}' {typeFilter}"));

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
                    $"[ManagementType] == '{Enums.ItemManagementType.FIFO}'"),
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
                    Resources.MasterData.LoadingUnitAll,
                    loadingUnitProvider),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaManual",
                    Resources.MasterData.LoadingUnitAreaManual,
                    loadingUnitProvider,
                    $"[AreaName] == 'Manual Area'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewAreaVertimag",
                    Resources.MasterData.LoadingUnitAreaVertimag,
                    loadingUnitProvider,
                    "[AreaName] == 'Vertimag Area'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusAvailable",
                    Resources.MasterData.LoadingUnitStatusAvailable,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Available'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusBlocked",
                    Resources.MasterData.LoadingUnitStatusBlocked,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Blocked'"),

                new PagedDataSource<LoadingUnit, int>(
                    "LoadingUnitsViewStatusUsed",
                    Resources.MasterData.LoadingUnitStatusUsed,
                    loadingUnitProvider,
                    "[LoadingUnitStatusDescription] == 'Used'"),
            }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IDataSource<TModel, TKey>> GetMachinesDataSources<TModel, TKey>()
                            where TModel : IModel<TKey>
        {
            var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

            const string vertimagXSFilter = "[Model] == 'VMAG/ver-2018/variant-XS/depth-103'";
            const string vertimagMFilter = "[Model] == 'VMAG/ver-2018/variant-M/depth-84'";

            return new List<DataSourceCollection<Machine, int>>
            {
                new DataSourceCollection<Machine, int>(
                    "MachinesViewAll",
                    Resources.Machines.MachineAll,
                    async () => await machineProvider.GetAllAsync(0, 0)),

                new DataSourceCollection<Machine, int>(
                    "MachinesViewVertimagXS",
                    Resources.Machines.MachineVertimagXS,
                    async () => await machineProvider.GetAllAsync(0, 0, null, vertimagXSFilter)),

                new DataSourceCollection<Machine, int>(
                    "MachinesViewVertimagM",
                    Resources.Machines.MachineVertimagM,
                    async () => await machineProvider.GetAllAsync(0, 0, null, vertimagMFilter)),
            }.Cast<IDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IDataSource<TModel, TKey>> GetMissionsDataSources<TModel, TKey>()
            where TModel : IModel<TKey>
        {
            var missionProvider = ServiceLocator.Current.GetInstance<IMissionProvider>();

            return new List<DataSourceCollection<Mission, int>>
            {
                new DataSourceCollection<Mission, int>(
                    "MissionViewAll",
                    Resources.Scheduler.MissionAll,
                    async () => await missionProvider.GetAllAsync(0, 0)),

                new DataSourceCollection<Mission, int>(
                    "MissionViewStatusCompleted",
                    Resources.Scheduler.MissionStatusCompleted,
                    async () => await missionProvider.GetAllAsync(0, 0, null, $"[Status] == '{Enums.MissionStatus.Completed}'")),

                new DataSourceCollection<Mission, int>(
                    "MissionViewStatusNew",
                    Resources.Scheduler.MissionStatusNew,
                    async () => await missionProvider.GetAllAsync(0, 0, null, $"[Status] == '{Enums.MissionStatus.New}'")),
            }.Cast<IDataSource<TModel, TKey>>();
        }

        private static IEnumerable<IFilterDataSource<TModel, TKey>> GetSchedulerRequestDataSources<TModel, TKey>()
                                            where TModel : IModel<TKey>
        {
            var schedulerRequestProvider = ServiceLocator.Current.GetInstance<ISchedulerRequestProvider>();

            return new List<PagedDataSource<SchedulerRequest, int>>
                    {
                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestViewAll",
                            Resources.Scheduler.SchedulerRequestViewAll,
                            schedulerRequestProvider),

                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationPut",
                            Common.Resources.BusinessObjects.Put,
                            schedulerRequestProvider,
                            $"[OperationType] == '{Enums.OperationType.Put}'"),

                        new PagedDataSource<SchedulerRequest, int>(
                            "SchedulerRequestOperationPick",
                            Common.Resources.BusinessObjects.Pick,
                            schedulerRequestProvider,
                            $"[OperationType] == '{Enums.OperationType.Pick}'"),
                    }.Cast<IFilterDataSource<TModel, TKey>>();
        }

        #endregion
    }
}
