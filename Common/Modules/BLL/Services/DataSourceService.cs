using System.Linq;
using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Models;
using System;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    // All EF Item DataSources
    public enum DataSourceType
    {
        ItemAll,
        ItemCode,        
        ItemDscription,
    }

    public class DataSourceService : IDataSourceService
    {
        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();

        #region Methods

        public IEnumerable<object> GetAll()
        {            
            var dataSources = new List<DataSource<Item>>();

            // Item All            
            Func<IQueryable<Item>, IQueryable<Item>> fFilter = (items) => items.Where(i => i.Code.Contains("code")).AsQueryable();
            Func<Func<IQueryable<Item>, IQueryable<Item>>, IQueryable<Item>> fDataItems = (filter) => this.dataService.GetData<Item>(filter);
            Func<Func<IQueryable<Item>, IQueryable<Item>>, int> fCount = (filter) => this.dataService.GetData<Item>(filter).Count();
            var dsItems = new DataSource<Item>() { SourceName = DataSourceType.ItemAll, Name = "All", Filter = fFilter, GetCount = fCount,  GetData = fDataItems };
            dataSources.Add(dsItems);

            // Item Code contain 7
            fFilter = (items) => items.Where(i => i.Code.Contains("7")).AsQueryable();
            dsItems = new DataSource<Item>() { SourceName = DataSourceType.ItemCode, Name = "Code contain 7", Filter = fFilter, GetCount = fCount, GetData = fDataItems };
            dataSources.Add(dsItems);

            // Item Description contain n_1
            fFilter = (items) => items.Where(i => i.Description.Contains("n_1")).AsQueryable();
            dsItems = new DataSource<Item>() { SourceName = DataSourceType.ItemDscription, Name = "Description", Filter = fFilter, GetCount = fCount, GetData = fDataItems };
            dataSources.Add(dsItems);

            // Item Code contain Code_72
            fFilter = (items) => items.Where(i => i.Code.Contains("Code_72")).AsQueryable();
            dsItems = new DataSource<Item>() { SourceName = DataSourceType.ItemCode, Name = "Code_72", Filter = fFilter, GetCount = fCount, GetData = fDataItems };
            dataSources.Add(dsItems);

            return dataSources as IEnumerable<object>;
        }

        #endregion Methods
    }
}
