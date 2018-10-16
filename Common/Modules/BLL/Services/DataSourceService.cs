using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IDataSource<TModel, TId>> GetAll<TModel, TId>(string viewName, object parameter = null) where TModel : IBusinessObject<TId>
        {
#pragma warning disable IDE0009
            switch (viewName)
            {
                case "ItemsView":
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

                case "ItemDetailsView":
                    var itemDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment, int>>
                    {
                        new DataSource<Compartment, int>(
                            "ItemDetailsView",
                            Resources.MasterData.CompartmentAll,
                            () => itemDetailsProvider.GetByItemId((int)parameter))
                    }.Cast<IDataSource<TModel, TId>>();

                case "CompartmentsView":
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment, int>>
                    {
                        new DataSource<Compartment, int>(
                            "CompartmentsViewAll",
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case "CellsView":
                    var cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

                    return new List<DataSource<Cell, int>>
                    {
                        new DataSource<Cell, int>(
                            "CellsViewAll",
                            Resources.MasterData.CellAll,
                            () => cellProvider.GetAll(),
                            () => cellProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                case "LoadingUnitsView":
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnit, int>>
                    {
                        new DataSource<LoadingUnit, int>(
                            "LoadingUnitsViewAll",
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitProvider.GetAllCount())
                    }.Cast<IDataSource<TModel, TId>>();

                default:
                    return new List<IDataSource<TModel, TId>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
