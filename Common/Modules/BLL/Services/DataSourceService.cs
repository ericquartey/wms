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
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IFilterDataSource<TModel>> GetAllFilters<TModel>(string viewModelName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewModelName)
            {
                case MasterData.ITEMS:
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

                case MasterData.COMPARTMENTS:
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
                    var compartmentCountProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<FilterDataSource<Compartment>>
                    {
                        new FilterDataSource<Compartment>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentCountProvider.GetAllCount())
                    }.Cast<IFilterDataSource<TModel>>();

                case MasterData.CELLS:
                    var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
                    var cellCountProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

                    return new List<FilterDataSource<Cell>>
                    {
                        new FilterDataSource<Cell>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellCountProvider.GetAllCount())
                    }.Cast<IFilterDataSource<TModel>>();

                case Machines.MACHINES:
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

                case MasterData.LOADINGUNITS:
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
                    var loadingUnitCountProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<FilterDataSource<LoadingUnit>>
                    {
                        new FilterDataSource<LoadingUnit>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitCountProvider.GetAllCount())
                    }.Cast<IFilterDataSource<TModel>>();

                case MasterData.ITEMLISTS:
                    var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
                    var itemListCountProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

                    return new List<FilterDataSource<ItemList>>
                    {
                        new FilterDataSource<ItemList>(
                            "ItemListViewAll",
                            Resources.MasterData.ItemListAll,
                            () => itemListProvider.GetAll(),
                            () => itemListCountProvider.GetAllCount()),

                        new FilterDataSource<ItemList>(
                            "ItemListViewStatusWaiting",
                            Resources.MasterData.ItemListStatusWaiting,
                            () => itemListProvider.GetWithStatusWaiting(),
                            () => itemListCountProvider.GetWithStatusWaitingCount()),

                        new FilterDataSource<ItemList>(
                            "ItemListViewStatusCompleted",
                            Resources.MasterData.ItemListStatusCompleted,
                            () => itemListProvider.GetWithStatusCompleted(),
                            () => itemListCountProvider.GetWithStatusCompletedCount()),

                        new FilterDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()),
                    }.Cast<IFilterDataSource<TModel>>();

                default:
                    return new List<IFilterDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
