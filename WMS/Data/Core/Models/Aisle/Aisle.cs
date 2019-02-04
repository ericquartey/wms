namespace Ferretto.WMS.Data.Core.Models
{
    public class Aisle : BaseModel<int>
    {
        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
