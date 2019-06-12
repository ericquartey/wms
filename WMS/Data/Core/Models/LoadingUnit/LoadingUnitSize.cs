namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitSize
    {
        #region Properties

        [Positive]
        public double Height { get; set; }

        [Positive]
        public double Length { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        [Positive]
        public double Width { get; set; }

        #endregion
    }
}
