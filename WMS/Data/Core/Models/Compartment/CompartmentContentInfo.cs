namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentContentInfo : BaseModel<int>
    {
        #region Properties

        public double? Height { get; set; }

        public int? MaxCapacity { get; set; }

        public int Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public double? Width { get; set; }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        #endregion
    }
}
