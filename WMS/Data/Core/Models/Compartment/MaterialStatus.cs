using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(MaterialStatus))]
    public class MaterialStatus : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        #endregion Properties
    }
}
