namespace Ferretto.Common.Modules.BLL.Models
{
    public class Enumeration<TId> : BusinessObject
    {
        #region Constructors

        public Enumeration(TId id, string description)
        {
            this.Id = id;
            this.Description = description;
        }

        #endregion Constructors

        #region Properties

        public string Description { get; }
        public TId Id { get; }

        #endregion Properties
    }
}
