namespace Ferretto.WMS.Data.Core.Models
{
    public class CreationErrorOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public CreationErrorOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
