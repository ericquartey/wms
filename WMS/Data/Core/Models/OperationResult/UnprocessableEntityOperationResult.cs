namespace Ferretto.WMS.Data.Core.Models
{
    public class UnprocessableEntityOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public UnprocessableEntityOperationResult()
            : base(false)
        {
        }

        public UnprocessableEntityOperationResult(string description)
            : base(description: description)
        {
        }

        #endregion
    }
}
