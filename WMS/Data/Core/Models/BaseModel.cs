using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BaseModel<TKey> : BasePolicyModel, IModel<TKey>
    {
        #region Constructors

        protected BaseModel()
        {
        }

        #endregion

        #region Properties

        public TKey Id { get; set; }

        #endregion
    }
}
