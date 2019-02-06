namespace Ferretto.WMS.Data.Core.Models
{
    public class NotCreatedOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public NotCreatedOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
