namespace Ferretto.WMS.Data.Core.Models
{
    public class CreationErrorOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public CreationErrorOperationResult()
            : base(false)
        {
        }

        public CreationErrorOperationResult(string description)
            : base(description: description)
        {
        }

        public CreationErrorOperationResult(System.Exception exception)
         : base(description: exception?.Message)
        {
        }

        #endregion
    }
}
