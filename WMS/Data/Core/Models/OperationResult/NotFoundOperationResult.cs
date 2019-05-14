namespace Ferretto.WMS.Data.Core.Models
{
    public class NotFoundOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public NotFoundOperationResult()
            : base(false)
        {
        }

        public NotFoundOperationResult(T model, string description)
            : base(model, description)
        {
            this.Description = description;
        }

        #endregion
    }
}
