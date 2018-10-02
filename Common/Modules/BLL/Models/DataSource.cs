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
        }

        public DataSource(string name, Func<IQueryable<TModel>> getData, Func<IQueryable<TModel>, int> getDataCount)
                    : this(name, getData)
        {
            if (getDataCount != null)
            {
                this.GetDataCount = getDataCount;
            }
            else
            {
                this.GetDataCount = data => this.GetData().Count();
            }
        }

        #endregion Constructors

        #region Properties

        public Func<IQueryable<TModel>> GetData { get; protected set; }

        public Func<IQueryable<TModel>, int> GetDataCount { get; protected set; }

        public string Name { get; private set; }

        #endregion Properties
    }
}
