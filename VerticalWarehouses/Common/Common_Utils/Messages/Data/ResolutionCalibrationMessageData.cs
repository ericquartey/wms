using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ResolutionCalibrationMessageData : IResolutionCalibrationMessageData
    {
        #region Constructors

        public ResolutionCalibrationMessageData(decimal readInitialPosition, decimal readFinalPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ReadInitialPosition = readInitialPosition;
            this.ReadFinalPosition = readFinalPosition;
        }

        #endregion

        #region Properties

        public decimal ReadFinalPosition { get; }

        public decimal ReadInitialPosition { get; }

        public decimal Resolution { get; set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
