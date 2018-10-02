using System.Collections.Generic;
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

        public IEnumerable<IDataSource<IBusinessObject>> GetAll(string viewName, object parameter = null)
        {
#pragma warning disable IDE0009
            switch (viewName)
            {
                case "ItemsView":
                    return (IEnumerable<IDataSource<IBusinessObject>>)new List<DataSource<Item>>
                    {
                        new DataSource<Item>(MasterData.ItemAll, () => this.businessProvider.GetAllItems()),
                        new DataSource<Item>(MasterData.ItemClassA, () => this.businessProvider.GetItemsWithAClass()),
                        new DataSource<Item>(MasterData.ItemFIFO, () => this.businessProvider.GetItemsWithFifo())
                    };

                case "ItemDetailsView":
                    return (IEnumerable<IDataSource<IBusinessObject>>)new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(MasterData.CompartmentAll, () => this.businessProvider.GetCompartmentsByItemId((int)parameter))
                    };

                case "CompartmentsView":
                    return (IEnumerable<IDataSource<IBusinessObject>>)new List<DataSource<Compartment>>
                    {
                        new DataSource<Compartment>(MasterData.CompartmentAll, () => this.businessProvider.GetAllCompartments())
                    };

                case "LoadingUnitsView":
                    return (IEnumerable<IDataSource<IBusinessObject>>)new List<DataSource<LoadingUnit>>
                    {
                        new DataSource<LoadingUnit>(MasterData.LoadingUnitAll, () => this.businessProvider.GetAllLoadingUnits())
                    };

                default:
                    return (IEnumerable<IDataSource<IBusinessObject>>)new List<object>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
