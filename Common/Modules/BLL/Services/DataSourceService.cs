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

        public IEnumerable<IDataSource<TModel>> GetAll<TModel>(string viewModelName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewModelName)
            {
                case MasterData.ITEMS:
                    var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    var itemsCountProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    return new List<DataSource<Item>>
                    {
                        new DataSource<Item>(
                            "ItemsViewAll",
                            Resources.MasterData.ItemAll,
                            () => itemsProvider.GetAll(),
                            () => itemsCountProvider.GetAllCount()),

                        new DataSource<Item>(
                            "ItemsViewClassA",
                            Resources.MasterData.ItemClassA,
                            () => itemsProvider.GetWithAClass(),
                            () => itemsCountProvider.GetWithAClassCount()),

                        new DataSource<Item>(
                            "ItemsViewFIFO",
                            Resources.MasterData.ItemFIFO,
                            () => itemsProvider.GetWithFifo(),
                            () => itemsCountProvider.GetWithFifoCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.ITEMDETAILS:
                    var itemDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(
                            "ItemDetailsView",
                            Resources.MasterData.CompartmentAll,
                            () => itemDetailsProvider.GetByItemId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case MasterData.COMPARTMENTS:
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
                    var compartmentCountProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentCountProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.COMPARTMENTDETAILS:
                    var itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

                    return new List<DataSource<AllowedItemInCompartment>>
                    {
                        new DataSource<AllowedItemInCompartment>(
                            "CompartmentDetailsView",
                            Resources.MasterData.ItemAll,
                            () => itemProvider.GetAllowedByCompartmentId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case MasterData.CELLS:
                    var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
                    var cellCountProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

                    return new List<DataSource<Cell>>
                    {
                        new DataSource<Cell>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellCountProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.CELLDETAILS:
                    var loadingUnitsProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnitDetails>>
                    {
                        new DataSource<LoadingUnitDetails>(
                            "CellDetailsView",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitsProvider.GetByCellId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case Machines.MACHINES:
                    var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();
                    var machineCountProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

                    return new List<DataSource<Machine>>
                    {
                        new DataSource<Machine>(
                            "MachinesViewAll",
                            Resources.Machines.MachineAll,
                            () => machineProvider.GetAll(),
                            () => machineCountProvider.GetAllCount()),

                          new DataSource<Machine>(
                            "MachinesViewVertimagXS",
                            Resources.Machines.MachineVertimagXS,
                            () => machineProvider.GetAllVertimagModelXS(),
                            () => machineCountProvider.GetAllVertimagModelXSCount()),

                          new DataSource<Machine>(
                            "MachinesViewVertimagM",
                            Resources.Machines.MachineVertimagM,
                            () => machineProvider.GetAllVertimagModelM(),
                            () => machineCountProvider.GetAllVertimagModelMCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.LOADINGUNITS:
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
                    var loadingUnitCountProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnit>>
                    {
                        new DataSource<LoadingUnit>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitCountProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.LOADINGUNITDETAILS:
                    var loadingUnitDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<CompartmentDetails>>
                    {
                        new DataSource<CompartmentDetails>(
                            "LoadingUnitDetailsView",
                            Resources.MasterData.CompartmentAll,
                            () => loadingUnitDetailsProvider.GetByLoadingUnitId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case MasterData.ITEMLISTS:
                    var itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
                    var itemListCountProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

                    return new List<DataSource<ItemList>>
                    {
                        new DataSource<ItemList>(
                            "ItemListViewAll",
                            Resources.MasterData.ItemListAll,
                            () => itemListProvider.GetAll(),
                            () => itemListCountProvider.GetAllCount()),

                        new DataSource<ItemList>(
                            "ItemListViewStatusWaiting",
                            Resources.MasterData.ItemListStatusWaiting,
                            () => itemListProvider.GetWithStatusWaiting(),
                            () => itemListCountProvider.GetWithStatusWaitingCount()),

                        new DataSource<ItemList>(
                            "ItemListViewStatusCompleted",
                            Resources.MasterData.ItemListStatusCompleted,
                            () => itemListProvider.GetWithStatusCompleted(),
                            () => itemListCountProvider.GetWithStatusCompletedCount()),

                        new DataSource<ItemList>(
                            "ItemListViewTypePick",
                            Resources.MasterData.ItemListsTypePick,
                            () => itemListProvider.GetWithTypePick(),
                            () => itemListCountProvider.GetWithTypePickCount()),
                    }.Cast<IDataSource<TModel>>();

                default:
                    return new List<IDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
