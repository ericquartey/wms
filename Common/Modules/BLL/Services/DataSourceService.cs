using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Utils.Modules;
using Microsoft.Practices.ServiceLocation;
using Compartment = Ferretto.Common.BusinessModels.Compartment;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IDataSource<TModel, TId>> GetAll<TModel, TId>(string viewModelName, object parameter = null) where TModel : IBusinessObject<TId>
        {
#pragma warning disable IDE0009
            switch (viewModelName)
            {
                case MasterData.ITEMS:
                    var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    return new List<DataSource<Item, int>>
                    {
                        new DataSource<Item, int>(
                            "ItemsViewAll",
                            Resources.MasterData.ItemAll,
                            () => itemsProvider.GetAll(),
                            () => itemsProvider.GetAllCount()),
                        new DataSource<Item, int>(
                            "ItemsViewClassA",
                            Resources.MasterData.ItemClassA,
                            () => itemsProvider.GetWithAClass(),
                            () => itemsProvider.GetWithAClassCount()),
                        new DataSource<Item, int>(
                            "ItemsViewFIFO",
                            Resources.MasterData.ItemFIFO,
                            () => itemsProvider.GetWithFifo(),
                            () => itemsProvider.GetWithFifoCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case MasterData.ITEMDETAILS:
                    var itemDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment, int>>
                    {
                        new DataSource<Compartment, int>(
                            "ItemDetailsView",
                            Resources.MasterData.CompartmentAll,
                            () => itemDetailsProvider.GetByItemId((int)parameter))
                    }.Cast<IDataSource<TModel, TId>>();

                case MasterData.COMPARTMENTS:
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment, int>>
                    {
                        new DataSource<Compartment, int>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case MasterData.CELLS:
                    var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

                    return new List<DataSource<Cell, int>>
                    {
                        new DataSource<Cell, int>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case Machines.MACHINES:
                    var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

                    return new List<DataSource<Machine, int>>
                    {
                        new DataSource<Machine, int>(
                            "MachinesViewAll",
                            Resources.Machines.MachineAll,
                            () => machineProvider.GetAll(),
                            () => machineProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case MasterData.LOADINGUNITS:
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnit, int>>
                    {
                        new DataSource<LoadingUnit, int>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case MasterData.LOADINGUNITDETAILS:
                    var loadingUnitDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<CompartmentDetails, int>>
                    {
                        new DataSource<CompartmentDetails, int>(
                            "LoadingUnitDetailsView",
                            Resources.MasterData.CompartmentAll,
                            () => loadingUnitDetailsProvider.GetByLoadingUnitId((int)parameter))
                    }.Cast<IDataSource<TModel, TId>>();

                default:
                    return new List<IDataSource<TModel, TId>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
