using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class PagedDataSource<TModel> : IFilterDataSource<TModel>
        where TModel : IBusinessObject
    {
        #region Constructors

        public PagedDataSource(string key, string name, IPagedBusinessProvider<TModel> provider, string expression)
        {
            this.Key = key;
            this.Name = name;
            this.Provider = provider;
            this.Expression = expression;
        }

        public PagedDataSource(string key, string name, IPagedBusinessProvider<TModel> provider)
            : this(key, name, provider, expression: null)
        {
        }

        #endregion Constructors

        #region Properties

        public string Expression { get; }

        public Func<IQueryable<TModel>> GetData { get; }

        public Func<int> GetDataCount { get; }

        public string Key { get; }

        public string Name { get; }

        public IPagedBusinessProvider<TModel> Provider { get; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion Methods
    }
}
