namespace Ferretto.WMS.Data.Core.Models
{
    public class AllowedItemArea : BaseModel<int>, IAreaDeleteItemArea
    {
        #region Properties

        public string Name { get; set; }

        public double TotalStock { get; set; }

        #endregion
    }
}
