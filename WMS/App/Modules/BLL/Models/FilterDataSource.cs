using System;
using System.Linq;
using DevExpress.Data.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class FilterDataSource<TModel, TKey> : EntityInstantFeedbackSource, IFilterDataSource<TModel, TKey>
        where TModel : IModel<TKey>
    {
        #region Constructors

        public FilterDataSource(string key, string name, Func<IQueryable<TModel>> getData)
                          : this(key, name, getData, () => getData().Count())
        {
        }

        public FilterDataSource(string key, string name, Func<IQueryable<TModel>> getData, Func<int> getDataCount)
                : base((a) => a.QueryableSource = getData())
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (getData == null)
            {
                throw new ArgumentNullException(nameof(getData));
            }

            if (getDataCount == null)
            {
                throw new ArgumentNullException(nameof(getDataCount));
            }

            this.Key = key;
            this.Name = name;
            this.GetData = getData;
            this.GetDataCount = getDataCount;

            this.KeyExpression = "Id";
        }

        #endregion

        #region Properties

        public string FilterString => null;

        public Func<IQueryable<TModel>> GetData { get; protected set; }

        public Func<int> GetDataCount { get; protected set; }

        public string Key { get; private set; }

        public string Name { get; private set; }

        public IPagedBusinessProvider<TModel, TKey> Provider => null;

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion
    }
}
