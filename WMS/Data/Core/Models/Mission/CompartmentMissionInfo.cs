using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class CompartmentMissionInfo
    {
        #region Properties

        [Positive]
        public double? Depth { get; set; }

        public int Id { get; set; }

        [Positive]
        public double? MaxCapacity { get; set; }

        [PositiveOrZero]
        public double Stock { get; set; }

        [Positive]
        public double? Width { get; set; }

        [PositiveOrZero]
        public double? XPosition { get; set; }

        [PositiveOrZero]
        public double? YPosition { get; set; }

        #endregion
    }
}
