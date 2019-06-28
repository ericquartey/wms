using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class OperationResult<TModel> : IOperationResult<TModel>
    {
        #region Constructors

        protected OperationResult(
             TModel entity = default(TModel),
             string description = null,
             bool showToast = true)
             : this(false)
        {
            this.Description = description;
            this.ShowToast = showToast;
        }

        protected OperationResult(
                    bool success,
            TModel entity = default(TModel),
            bool showToast = true)
        {
            this.Success = success;
            this.Entity = entity;
            this.ShowToast = showToast;
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public TModel Entity { get; private set; }

        public bool ShowToast { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
