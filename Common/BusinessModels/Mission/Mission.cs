namespace Ferretto.Common.BusinessModels
{
    public class Mission : BusinessObject
    {
        #region Properties

        public int BayId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string TypeId { get; set; }

        #endregion Properties
    }
}
