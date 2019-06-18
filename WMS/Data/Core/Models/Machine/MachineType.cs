using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(MachineType))]
    public class MachineType : BaseModel<string>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
