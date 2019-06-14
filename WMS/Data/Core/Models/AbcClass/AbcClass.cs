using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(AbcClass))]
    public class AbcClass : BaseModel<string>
    {
        #region Properties

        public string Description { get; set; }

        #endregion
    }
}
