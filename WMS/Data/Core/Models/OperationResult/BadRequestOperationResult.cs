using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BadRequestOperationResult<TModel> : OperationResult<TModel>
    {
        #region Constructors

        public BadRequestOperationResult(TModel model = default(TModel))
            : base(false, default(TModel))
        {
        }

        public BadRequestOperationResult(Exception ex)
            : base(default(TModel), ex?.Message)
        {
        }

        public BadRequestOperationResult(TModel model, string description)
             : base(model, description)
        {
            this.Description = description;
        }

        #endregion
    }
}
