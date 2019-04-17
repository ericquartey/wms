namespace Ferretto.WMS.App.Core.Models
{
    public sealed class CompartmentType : BusinessObject
    {
        #region Properties

        public double? Height { get; set; }

        public int TotalCompartments { get; set; }

        public int TotalEmptyCompartments { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
