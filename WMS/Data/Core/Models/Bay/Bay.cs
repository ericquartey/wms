using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Bay))]
    public class Bay : BaseModel<int>
    {
        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string BayTypeDescription { get; set; }

        public string BayTypeId { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        [Positive]
        public int? LoadingUnitsBufferSize { get; set; }

        public int? MachineId { get; set; }

        public string MachineNickname { get; set; }

        #endregion
    }
}
