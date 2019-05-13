namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
