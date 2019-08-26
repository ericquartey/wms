using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay
    {


        #region Properties

        public int? CurrentMissionId { get; set; }

        public int? CurrentMissionOperationId { get; set; }

        public int ExternalId { get; set; }

        public BayIndex Index { get; set; }

        public string IpAddress { get; set; }

        public bool IsActive { get; set; }

        public BayStatus Status
        {
            get
            {
                if (this.IsActive)
                {
                    return this.CurrentMissionOperationId.HasValue ? BayStatus.Busy : BayStatus.Idle;
                }

                return BayStatus.Disconnected;
            }
        }

        public BayType Type { get; set; }

        #endregion
    }
}
