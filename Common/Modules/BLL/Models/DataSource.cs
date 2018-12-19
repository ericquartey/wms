using System;
using System.Linq;
using DevExpress.Data.Linq;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class DataSource<TModel> : EntityInstantFeedbackSource, IDataSource<TModel>
        where TModel : IBusinessObject
    {
        #region Constructors

        public DataSource(Func<IQueryable<TModel>> getData)
                          : this(getData, () => getData().Count())
        { }

        public DataSource(Func<IQueryable<TModel>> getData, Func<int> getDataCount)
                : base((a) => a.QueryableSource = getData())
        {
            this.KeyExpression = "Id";
        }

        #endregion Constructors
    }
}
