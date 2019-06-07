using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class ItemMissionInfo
    {
        #region Properties

        public string Code { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public string Image { get; set; }

        #endregion
    }
}
