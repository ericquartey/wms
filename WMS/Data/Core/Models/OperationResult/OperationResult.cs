using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class OperationResult<TModel> : IOperationResult<TModel>
    {
        #region Constructors

        protected OperationResult(
             TModel entity = default(TModel),
             string description = null)
             : this(false)
        {
            this.Description = description;
        }

        protected OperationResult(
                    bool success,
                    TModel entity = default(TModel))
        {
            this.Success = success;
            this.Entity = entity;
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public TModel Entity { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
