namespace Ferretto.WMS.App.Core.Models
{
    public class LoadingUnitType
    {
        #region Properties

        public string Description { get; set; }

        public int HasCompartments { get; set; }

        public int LoadingUnitHeightClassId { get; set; }

        public int LoadingUnitSizeClassId { get; set; }

        public int LoadingUnitWeightClassId { get; set; }

        #endregion
    }
}
