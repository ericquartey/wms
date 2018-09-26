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
    }

    public class DataSourceService : IDataSourceService
    {
        #region Fields

        private readonly IBusinessProvider businessService = ServiceLocator.Current.GetInstance<BusinessProvider>();

        #endregion Fields



        #region Methods

        public IEnumerable<object> GetAll(string viewName)
        {
#pragma warning disable IDE0009
            switch (viewName)
            {
                case "ItemsView":
                    return new List<DataSource<Item>>
                    {
                        // All items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemAll,
                            Name = MasterData.ItemAll,
                            GetCount = filter => this.businessService.GetAllItemsCount(),
                            GetData = filter => this.businessService.GetAllItems()
                        },
                        //// A-Class items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemAClass,
                            Name = MasterData.ItemClassA,
                            GetCount = filter => this.businessService.GetItemsWithAClassCount(),
                            GetData = filter => this.businessService.GetItemsWithAClass()
                        },
                        //// FIFO items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemFifo,
                            Name = MasterData.ItemFIFO,
                            GetCount = filter => this.businessService.GetItemsWithFIFOCount(),
                            GetData = filter => this.businessService.GetItemsWithFIFO()
                        }
                    };

                case "CompartmentsView":
                    return new List<DataSource<Compartment>>
                    {
                        // All compartments
                        new DataSource<Compartment>
                        {
                            SourceName = DataSourceType.CompartmentAll,
                            Name = MasterData.CompartmentAll,
                            Filter = compartments => compartments,
                            GetCount = filter => this.businessService.GetAllCompartmentsCount(),
                            GetData = filter => this.businessService.GetAllCompartments()
                        }
                    };

                default:
                    return new List<object>();
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
