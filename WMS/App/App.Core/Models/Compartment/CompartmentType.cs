namespace Ferretto.WMS.App.Core.Models
{
    public sealed class CompartmentType : BusinessObject
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
