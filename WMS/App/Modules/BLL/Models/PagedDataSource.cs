using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class PagedDataSource<TModel, TKey> : IFilterDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Constructors

        public PagedDataSource(string key, string name, IPagedBusinessProvider<TModel, TKey> provider, string filterString)
        {
            this.Key = key;
            this.Name = name;
            this.Provider = provider;
            this.FilterString = filterString;
        }

        public PagedDataSource(string key, string name, IPagedBusinessProvider<TModel, TKey> provider)
            : this(key, name, provider, null)
        {
        }

        #endregion

        #region Properties

        public string FilterString { get; }

        public Func<IQueryable<TModel>> GetData { get; }

        public Func<int> GetDataCount { get; }

        public string Key { get; }

        public string Name { get; }

        public IPagedBusinessProvider<TModel, TKey> Provider { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion
    }
}
