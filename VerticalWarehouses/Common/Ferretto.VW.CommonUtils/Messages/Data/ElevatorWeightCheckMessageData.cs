using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ElevatorWeightCheckMessageData : IElevatorWeightCheckMessageData
    {
        #region Properties

        public MessageVerbosity Verbosity { get; set; }

        public decimal? Weight { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Weight:{this.Weight}";
        }

        #endregion
    }
}
