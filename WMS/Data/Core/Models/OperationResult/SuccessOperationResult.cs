namespace Ferretto.WMS.Data.Core.Models
{
    public class SuccessOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public SuccessOperationResult()
            : base(true)
        {
        }

        public SuccessOperationResult(T model)
            : base(true, model)
        {
        }

        #endregion
    }
}
