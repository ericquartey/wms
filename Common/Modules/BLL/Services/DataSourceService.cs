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

        public IEnumerable<IDataSource<TModel>> GetAll<TModel>(string viewModelName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewModelName)
            {
                case MasterData.ITEMS:
                    var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    return new List<DataSource<Item>>
                    {
                        new DataSource<Item>(
                            "ItemsViewAll",
                            Resources.MasterData.ItemAll,
                            () => itemsProvider.GetAll(),
                            () => itemsProvider.GetAllCount()),
                        new DataSource<Item>(
                            "ItemsViewClassA",
                            Resources.MasterData.ItemClassA,
                            () => itemsProvider.GetWithAClass(),
                            () => itemsProvider.GetWithAClassCount()),
                        new DataSource<Item>(
                            "ItemsViewFIFO",
                            Resources.MasterData.ItemFIFO,
                            () => itemsProvider.GetWithFifo(),
                            () => itemsProvider.GetWithFifoCount())
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

                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentProvider.GetAllCount())
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

                    return new List<DataSource<Cell>>
                    {
                        new DataSource<Cell>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case Machines.MACHINES:
                    var machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

                    return new List<DataSource<Machine>>
                    {
                        new DataSource<Machine>(
                            "MachinesViewAll",
                            Resources.Machines.MachineAll,
                            () => machineProvider.GetAll(),
                            () => machineProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case MasterData.LOADINGUNITS:
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnit>>
                    {
                        new DataSource<LoadingUnit>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitProvider.GetAllCount())
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

                default:
                    return new List<IDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
