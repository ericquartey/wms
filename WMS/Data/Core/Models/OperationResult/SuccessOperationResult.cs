namespace Ferretto.WMS.Data.Core.Models
{
    public class SuccessOperationResult<TModel> : OperationResult<TModel>
    {
        #region Constructors

        public SuccessOperationResult()
            : base(true)
        {
        }

        public SuccessOperationResult(TModel model)
            : base(true, model)
        {
        }

        #endregion
    }
}
