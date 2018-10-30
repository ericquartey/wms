namespace Ferretto.Common.BusinessModels
{
    public enum MissionType { Withdrawal, Insertion }

    public class Mission
    {
        #region Properties

        public int BayId { get; set; }
        public int Id { get; set; }
        public int Quantity { get; set; }
        public MissionType Type { get; set; }
        public System.Object ItemId { get; set; }

        #endregion Properties
    }
}
