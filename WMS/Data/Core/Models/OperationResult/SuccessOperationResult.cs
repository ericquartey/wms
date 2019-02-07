namespace Ferretto.WMS.Data.Core.Models
{
    public class SuccessOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public SuccessOperationResult(T model)
            : base(true, model)
        {
        }

        #endregion
    }
}
