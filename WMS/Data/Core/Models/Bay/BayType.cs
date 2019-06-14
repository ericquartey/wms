using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(BayType))]
    public class BayType : BaseModel<string>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
