using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class WeightAcquisitionCommandMessageData : IMessageData
    {
        #region Properties

        public CommandAction CommandAction { get; set; }

        public double Displacement { get; set; }

        public double InitialPosition { get; set; }

        public TimeSpan InPlaceSamplingDuration { get; set; }

        public double NetWeight { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
