using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.Aisle))]
    public class Aisle : BusinessObject
    {
        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
