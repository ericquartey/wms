using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnitStatus))]
    public class LoadingUnitStatus : BaseModel<string>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
