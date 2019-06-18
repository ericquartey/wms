using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Machine))]
    public class MachinePick : BaseModel<int>
    {
        #region Properties

        [PositiveOrZero]
        public double? AvailableQuantityItem { get; set; }

        public string Nickname { get; set; }

        #endregion
    }
}
