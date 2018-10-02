using System;
using System.Linq;
using DevExpress.Data.Linq;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class DataSource<TModel> : EntityInstantFeedbackSource, IDataSource<TModel>
        where TModel : BusinessObject
    {
        #region Constructors

        public DataSource(string name, Func<IQueryable<TModel>> getData)
                          : this(name, getData, () => getData().Count())
        { }

        public DataSource(string name, Func<IQueryable<TModel>> getData, Func<int> getDataCount)
                : base((a) => a.QueryableSource = getData())
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.KeyExpression = "Id";
            this.AreSourceRowsThreadSafe = true;
            this.GetData = getData;

            if (getDataCount == null)
            {
                throw new ArgumentNullException(nameof(getDataCount));
            }
            else
            {
                this.GetDataCount = getDataCount;
            }
        }

        #endregion Constructors

        #region Properties

        public Func<IQueryable<TModel>> GetData { get; protected set; }

        public Func<int> GetDataCount { get; protected set; }

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
