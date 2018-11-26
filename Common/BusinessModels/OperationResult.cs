namespace Ferretto.Common.BusinessModels
{
    public class OperationResult
    {
        #region Constructors

        public OperationResult(bool success, string description)
        {
            this.Success = success;
            this.Description = description;
        }

        public OperationResult(bool success) : this(success, null)
        { }

        #endregion Constructors

        #region Properties

        public string Description { get; private set; }
        public bool Success { get; private set; }

        #endregion Properties
    }
}
