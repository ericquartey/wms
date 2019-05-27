namespace Ferretto.WMS.Data.Core.Models
{
    public class AlreadyCreatedOperationResult<TModel> : OperationResult<TModel>
    {
        #region Constructors

        public AlreadyCreatedOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
