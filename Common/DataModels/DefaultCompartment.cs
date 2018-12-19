namespace Ferretto.Common.DataModels
{
    // Scomparti predefiniti
    public sealed class DefaultCompartment : IDataModel
    {
        #region Properties

        public CompartmentType CompartmentType { get; set; }

        public int CompartmentTypeId { get; set; }

        public DefaultLoadingUnit DefaultLoadingUnit { get; set; }

        public int DefaultLoadingUnitId { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        public string Note { get; set; }

        public int XPosition { get; set; }

        public int YPosition { get; set; }

        #endregion Properties
    }
}
