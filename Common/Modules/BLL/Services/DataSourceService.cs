using System.Collections.Generic;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
    // All EF Item DataSources
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

        private readonly BusinessProvider businessProvider = new BusinessProvider();

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
                            GetCount = filter => this.businessProvider.GetAllItemsCount(),
                            GetData = filter => this.businessProvider.GetAllItems()
                        },
                        //// A-Class items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemAClass,
                            Name = MasterData.ItemClassA,
                            GetCount = filter => this.businessProvider.GetItemsWithAClassCount(),
                            GetData = filter => this.businessProvider.GetItemsWithAClass()
                        },
                        //// FIFO items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemFifo,
                            Name = MasterData.ItemFIFO,
                            GetCount = filter => this.businessProvider.GetItemsWithFIFOCount(),
                            GetData = filter => this.businessProvider.GetItemsWithFIFO()
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
                            GetCount = filter => this.businessProvider.GetCompartmentsCount(),
                            GetData = filter => this.businessProvider.GetAllCompartments()
                        }
                    };

                default:
                    return null;
            }
#pragma warning restore IDE0009
        }

        #endregion Methods
    }
}
