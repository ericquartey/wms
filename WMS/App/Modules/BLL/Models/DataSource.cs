using System;
using System.Linq;
using DevExpress.Data.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.WMS.App.Modules.BLL
{
    public class DataSource<TModel, TKey> : EntityInstantFeedbackSource, IDataSource<TModel, TKey>
        where TModel : IModel<TKey>
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
