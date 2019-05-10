using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BadRequestOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public BadRequestOperationResult(T model)
            : base(true, model)
        {
        }

        public BadRequestOperationResult(T model, string description)
             : base(model, description)
        {
            this.Description = description;
        }

        #endregion
    }
}
