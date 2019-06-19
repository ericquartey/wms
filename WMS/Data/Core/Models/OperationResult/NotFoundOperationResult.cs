namespace Ferretto.WMS.Data.Core.Models
{
    public class NotFoundOperationResult<TModel> : OperationResult<TModel>
    {
        #region Constructors

        public NotFoundOperationResult()
            : base(false)
        {
        }

        public NotFoundOperationResult(TModel model, string description)
            : base(model, description)
        {
            this.Description = description;
        }

        #endregion
    }
}
