using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay
    {
        #region Properties

        public int ExternalId { get; set; }

        public int Id { get; set; }

        public string IpAddress { get; set; }

        public bool IsActive { get; set; }

        public BayStatus Status { get; set; }

        public BayType Type { get; set; }

        #endregion
    }
}
