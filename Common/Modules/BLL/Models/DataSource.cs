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
            : base((a) => a.QueryableSource = getData())
        {
            this.KeyExpression = "Id";
        }

        #endregion
    }
}
