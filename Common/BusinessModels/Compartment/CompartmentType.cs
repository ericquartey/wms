namespace Ferretto.Common.BusinessModels
{
    public sealed class CompartmentType : BusinessObject
    {
        #region Properties

        public string Description { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }

        #endregion Properties
    }
}
