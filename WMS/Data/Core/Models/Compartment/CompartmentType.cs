namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentType : BaseModel<int>
    {
        #region Properties

        public double? Height { get; set; }

        public int TotalCompartments { get; set; }

        public int TotalEmptyCompartments { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
