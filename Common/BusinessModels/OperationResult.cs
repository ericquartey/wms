namespace Ferretto.Common.BusinessModels
{
    public class OperationResult
    {
        #region Constructors

        public OperationResult(bool success, int? entityId = null, string description = null)
        {
            this.Success = success;
            this.Description = description;
            this.EntityId = entityId;
        }

        #endregion Constructors

        #region Properties

        public string Description { get; private set; }

        public int? EntityId { get; private set; }

        public bool Success { get; private set; }

        #endregion Properties
    }
}
