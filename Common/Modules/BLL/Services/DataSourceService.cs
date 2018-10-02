using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public enum DataSourceType
    {
        ItemAll,
        ItemAClass,
        ItemFifo,

        CompartmentAll,
        ItemCompartments,

        LoadingUnitAll,
    }

    public class DataSourceService : IDataSourceService
    {
        #region Fields

        private readonly IBusinessProvider businessProvider = ServiceLocator.Current.GetInstance<IBusinessProvider>();

        #endregion Fields

        #region Methods

        public IEnumerable<IDataSource<TModel>> GetAll<TModel>(string viewName, object parameter = null) where TModel : IBusinessObject
        {
#pragma warning disable IDE0009
            switch (viewName)
            {
                case "ItemsView":
                    return new List<DataSource<Item>>
                    {
                        new DataSource<Item>(MasterData.ItemAll, () => this.businessProvider.GetAllItems()),
                        new DataSource<Item>(MasterData.ItemClassA, () => this.businessProvider.GetItemsWithAClass()),
                        new DataSource<Item>(MasterData.ItemFIFO, () => this.businessProvider.GetItemsWithFifo())
                    }.Cast<IDataSource<TModel>>();

                case "ItemDetailsView":
                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(MasterData.CompartmentAll, () => this.businessProvider.GetCompartmentsByItemId((int)parameter))
                    }.Cast<IDataSource<TModel>>();

                case "CompartmentsView":
                    return new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(MasterData.CompartmentAll, () => this.businessProvider.GetAllCompartments())
                    }.Cast<IDataSource<TModel>>();

                case "LoadingUnitsView":
                    return new List<DataSource<LoadingUnit>>
                    {
                        new DataSource<LoadingUnit>(MasterData.LoadingUnitAll, () => this.businessProvider.GetAllLoadingUnits())
                    }.Cast<IDataSource<TModel>>();

                default:
                    return new List<IDataSource<TModel>>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
