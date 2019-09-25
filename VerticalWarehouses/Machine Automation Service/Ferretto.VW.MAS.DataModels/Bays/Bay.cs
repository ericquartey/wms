using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay : DataModel
    {
        #region Properties

        public int? CurrentMissionId { get; set; }

        public int? CurrentMissionOperationId { get; set; }

        public Inverter Inverter { get; set; }

        public IoDevice IoDevice { get; set; }

        public bool IsActive { get; set; }

        public bool IsExternal { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public BayNumber Number { get; set; }

        public BayOperation Operation { get; set; }

        public IEnumerable<BayPosition> Positions { get; set; }

        public Shutter Shutter { get; set; }

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
