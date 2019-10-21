using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Bay : DataModel
    {
        #region Properties

        public Carousel Carousel { get; set; }

        public double ChainOffset { get; set; }

        public int? CurrentMissionId { get; set; }

        public int? CurrentMissionOperationId { get; set; }

        public Inverter Inverter { get; set; }

        public IoDevice IoDevice { get; set; }

        public bool IsActive { get; set; }

        public bool IsDouble => this.Positions?.Count() == 2;

        public bool IsExternal { get; set; }

        public BayNumber Number { get; set; }

        public BayOperation Operation { get; set; }

        public IEnumerable<BayPosition> Positions { get; set; }

        public double Resolution { get; set; }

        public Shutter Shutter { get; set; }

        public WarehouseSide Side { get; set; }

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

        #endregion
    }
}
