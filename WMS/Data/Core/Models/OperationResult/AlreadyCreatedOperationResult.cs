namespace Ferretto.WMS.Data.Core.Models
{
    public class AlreadyCreatedOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public AlreadyCreatedOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
