using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class DataSourceCollection<TModel, TKey> : BindingList<TModel>, IDataSource<TModel, TKey>, IRefreshableDataSource
        where TModel : class, IModel<TKey>
    {
        #region Constructors

        public DataSourceCollection(string key, string name, Func<Task<IEnumerable<TModel>>> getDataAsync)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.GetDataAsync = getDataAsync;
            this.Key = key;
            this.Name = name;
        }

        public DataSourceCollection(string key, string name, IEnumerable<TModel> data)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.Key = key;
            this.Name = name;

            this.RaiseListChangedEvents = false;
            foreach (var item in data)
            {
                this.Add(item);
            }

            this.RaiseListChangedEvents = true;
        }

        #endregion

        #region Properties

        public Func<Task<IEnumerable<TModel>>> GetDataAsync { get; }

        public string Key { get; }

        public string Name { get; }

        #endregion

        #region Methods

        public async Task RefreshAsync()
        {
            if (this.GetDataAsync == null)
            {
                return;
            }

            var data = await this.GetDataAsync();

            this.RaiseListChangedEvents = false;
            this.Clear();
            foreach (var item in data)
            {
                this.Add(item);
            }

            this.RaiseListChangedEvents = true;

            this.ResetBindings();
        }

        #endregion
    }
}
