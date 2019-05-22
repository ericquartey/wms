using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BadRequestOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public BadRequestOperationResult(T model = default(T))
            : base(false, default(T))
        {
        }

        public BadRequestOperationResult(Exception ex)
            : base(default(T), ex?.Message)
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
