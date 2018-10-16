namespace Ferretto.Common.BusinessModels
{
    public class Enumeration<TId> : BusinessObject<TId>
    {
        #region Constructors

        public Enumeration(TId id, string description) : base(id)
        {
            this.Description = description;
        }

        #endregion Constructors

        #region Properties

        public string Description { get; }

        #endregion Properties
    }
}
