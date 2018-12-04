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

        public IEnumerable<ITileDataSource<TModel>> GetAllTiles<TModel>(string viewModelName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewModelName)
            {
                case MasterData.ITEMS:
                    var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    var itemsCountProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    return new List<TileDataSource<Item>>
                    {
                        new TileDataSource<Item>(
                            "ItemsViewAll",
                            Resources.MasterData.ItemAll,
                            () => itemsProvider.GetAll(),
                            () => itemsCountProvider.GetAllCount()),

                        new TileDataSource<Item>(
                            "ItemsViewClassA",
                            Resources.MasterData.ItemClassA,
                            () => itemsProvider.GetWithAClass(),
                            () => itemsCountProvider.GetWithAClassCount()),

                        new TileDataSource<Item>(
                            "ItemsViewFIFO",
                            Resources.MasterData.ItemFIFO,
                            () => itemsProvider.GetWithFifo(),
                            () => itemsCountProvider.GetWithFifoCount())
                    }.Cast<ITileDataSource<TModel>>();

                case MasterData.COMPARTMENTS:
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
                    var compartmentCountProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<TileDataSource<Compartment>>
                    {
                        new TileDataSource<Compartment>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentCountProvider.GetAllCount())
                    }.Cast<ITileDataSource<TModel>>();

                case MasterData.CELLS:
                    var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
                    var cellCountProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

                    return new List<TileDataSource<Cell>>
                    {
                        new TileDataSource<Cell>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellCountProvider.GetAllCount())
                    }.Cast<ITileDataSource<TModel>>();

                case Machines.MACHINES:
                    var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();
                    var machineCountProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

                    return new List<TileDataSource<Machine>>
                    {
                        new TileDataSource<Machine>(
                            "MachinesViewAll",
                            Resources.Machines.MachineAll,
                            () => machineProvider.GetAll(),
                            () => machineCountProvider.GetAllCount()),

                          new TileDataSource<Machine>(
                            "MachinesViewVertimagXS",
                            Resources.Machines.MachineVertimagXS,
                            () => machineProvider.GetAllVertimagModelXS(),
                            () => machineCountProvider.GetAllVertimagModelXSCount()),

                          new TileDataSource<Machine>(
                            "MachinesViewVertimagM",
                            Resources.Machines.MachineVertimagM,
                            () => machineProvider.GetAllVertimagModelM(),
                            () => machineCountProvider.GetAllVertimagModelMCount())
                    }.Cast<ITileDataSource<TModel>>();

                case MasterData.LOADINGUNITS:
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
                    var loadingUnitCountProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<TileDataSource<LoadingUnit>>
                    {
                        new TileDataSource<LoadingUnit>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitCountProvider.GetAllCount())
                    }.Cast<ITileDataSource<TModel>>();

                case MasterData.ITEMLISTS:
                    var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
                    var itemListCountProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

                    return new List<TileDataSource<ItemList>>
                    {
                        new TileDataSource<ItemList>(
                            "ItemListViewAll",
                            Resources.MasterData.ItemListAll,
                            () => itemListProvider.GetAll(),
                            () => itemListCountProvider.GetAllCount()),

                        new TileDataSource<ItemList>(
                            "ItemListViewStatusWaiting",
                            Resources.MasterData.ItemListStatusWaiting,
                            () => itemListProvider.GetWithStatusWaiting(),
                            () => itemListCountProvider.GetWithStatusWaitingCount()),

                        new TileDataSource<ItemList>(
                            "ItemListViewStatusCompleted",
                            Resources.MasterData.ItemListStatusCompleted,
                            () => itemListProvider.GetWithStatusCompleted(),
                            () => itemListCountProvider.GetWithStatusCompletedCount()),

                        new TileDataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()),
                    }.Cast<ITileDataSource<TModel>>();

                default:
                    return new List<ITileDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
