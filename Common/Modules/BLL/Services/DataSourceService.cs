using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Models;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;

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

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();

        #endregion Fields

        #region Methods

        public IEnumerable<object> GetAll(string viewName)
        {
            switch (viewName)
            {
                case "ItemsView":
                    return new List<DataSource<Item>>
                    {
                        // All items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemAll,
                            Name = Catalog.ItemAll,
                            Filter = items => items,
                            GetCount = filter => this.dataService.GetData(filter).Count(),
                            GetData = filter => this.dataService.GetData(filter)
                        },
                        // A-Class items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemAClass,
                            Name = Catalog.ItemClassA,
                            Filter = items => items.Where(item => item.AbcClass.Id == "A"),
                            GetCount = filter => this.dataService.GetData(filter).Count(),
                            GetData = filter => this.dataService.GetData(filter)
                        },
                        // FIFO items
                        new DataSource<Item>
                        {
                            SourceName = DataSourceType.ItemFifo,
                            Name = Catalog.ItemFIFO,
                            Filter = items => items.Where(item => item.ItemManagementType.Description.Contains("FIFO")),
                            GetCount = filter => this.dataService.GetData(filter).Count(),
                            GetData = filter => this.dataService.GetData(filter)
                        }
                    };

                case "CompartmentsView":
                    return new List<DataSource<Compartment>>
                    {
                        // All compartments
                        new DataSource<Compartment>
                        {
                            SourceName = DataSourceType.CompartmentAll,
                            Name = Catalog.CompartmentAll,
                            Filter = compartments => compartments,
                            GetCount = filter => this.dataService.GetData(filter).Count(),
                            GetData = filter => this.dataService.GetData(filter)
                        }
                    };
                default:
                    return null;
            }
        }

        #endregion Methods
    }
}
