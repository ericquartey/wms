using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(PackageType))]
    public class PackageType : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion Properties
    }
}
