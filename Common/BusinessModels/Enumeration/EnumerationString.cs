namespace Ferretto.Common.BusinessModels
{
    public class EnumerationString : BindableBase
    {
        #region Constructors

        public EnumerationString(string id, string description)
        {
            this.Description = description;
            this.Id = id;
        }

        #endregion Constructors

        #region Properties

        public string Description { get; }

        public string Id { get; set; }

        #endregion Properties
    }
}
