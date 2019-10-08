using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class WeightAcquisitionCommandMessageData : IMessageData
    {
        #region Properties

        public CommandAction CommandAction { get; set; }

        public decimal Displacement { get; set; }

        public decimal InitialPosition { get; set; }

        public TimeSpan InPlaceSamplingDuration { get; set; }

        public decimal NetWeight { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
