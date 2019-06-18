namespace Ferretto.WMS.Data.Hubs.Models
{
    public class BayStatus
    {
        #region Properties

        public int BayId { get; set; }

        public int? LoadingUnitId { get; set; }

        public int? LoggedUserId { get; set; }

        #endregion
    }
}
