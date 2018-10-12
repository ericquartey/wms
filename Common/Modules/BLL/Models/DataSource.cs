using System;
using System.Linq;
using DevExpress.Data.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class DataSource<TModel, TId> : EntityInstantFeedbackSource, IDataSource<TModel, TId>
        where TModel : BusinessObject<TId>
    {
        #region Constructors

        public DataSource(string key, string name, Func<IQueryable<TModel>> getData)
                          : this(key, name, getData, () => getData().Count())
        { }

        public DataSource(string key, string name, Func<IQueryable<TModel>> getData, Func<int> getDataCount)
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

        #endregion Constructors

        #region Properties

        public Func<IQueryable<TModel>> GetData { get; protected set; }

        public Func<int> GetDataCount { get; protected set; }

        public string Key { get; private set; }

        public string Name { get; private set; }

        #endregion Properties

        #region Methods

        public override String ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion Methods
    }
}
