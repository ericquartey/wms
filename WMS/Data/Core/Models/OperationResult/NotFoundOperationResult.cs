namespace Ferretto.WMS.Data.Core.Models
{
    public class NotFoundOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public NotFoundOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
