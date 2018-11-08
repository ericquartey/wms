namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class WithdrawRequest
    {
        #region Properties

        public int BayId { get; set; }

        public int ItemId { get; set; }

        public string Lot { get; set; }

        public int Quantity { get; set; }

        public string RegistrationNumber { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion Properties
    }
}
