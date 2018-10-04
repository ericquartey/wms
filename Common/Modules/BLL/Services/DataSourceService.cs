using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class DataSourceService : IDataSourceService
    {
        #region Methods

        public IEnumerable<IDataSource<TModel>> GetAll<TModel>(string viewName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewName)
            {
                case "ItemsView":
                    var itemsProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
                    return new List<DataSource<Item>>
                    {
                        new DataSource<Item>(
                            Resources.MasterData.ItemAll,
                            () => itemsProvider.GetAll(),
                            () => itemsProvider.GetAllCount()),
                        new DataSource<Item>(
                            Resources.MasterData.ItemClassA,
                            () => itemsProvider.GetWithAClass(),
                            () => itemsProvider.GetWithAClassCount()),
                        new DataSource<Item>(
                            Resources.MasterData.ItemFIFO,
                            () => itemsProvider.GetWithFifo(),
                            () => itemsProvider.GetWithFifoCount())
                    }.Cast<IDataSource<TModel>>();

                case "ItemDetailsView":
                    var itemDetailsProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(
                            Resources.MasterData.CompartmentAll,
                            () => itemDetailsProvider.GetByItemId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case "CompartmentsView":
                    var compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(
                            Resources.MasterData.CompartmentAll,
                            () => compartmentProvider.GetAll(),
                            () => compartmentProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                case "LoadingUnitsView":
                    var loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

                    return new List<DataSource<LoadingUnit>>
                    {
                        new DataSource<LoadingUnit>(
                            Resources.MasterData.LoadingUnitAll,
                            () => loadingUnitProvider.GetAll(),
                            () => loadingUnitProvider.GetAllCount())
                    }.Cast<IDataSource<TModel>>();

                default:
                    return new List<IDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
