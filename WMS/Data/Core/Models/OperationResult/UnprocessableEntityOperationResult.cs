namespace Ferretto.WMS.Data.Core.Models
{
    public class UnprocessableEntityOperationResult<TModel> : OperationResult<TModel>
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

        public UnprocessableEntityOperationResult(System.Exception exception)
            : base(description: exception?.Message)
        {
        }

        #endregion
    }
}
