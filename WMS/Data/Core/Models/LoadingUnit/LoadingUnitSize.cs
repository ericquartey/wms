using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnitSize))]
    public class LoadingUnitSize
    {
        #region Properties

        [Positive]
        public double Height { get; set; }

        [Positive]
        public double Depth { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        [Positive]
        public double Width { get; set; }

        #endregion
    }
}
