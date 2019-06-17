using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ResolutionCalibrationMessageData : IResolutionCalibrationMessageData
    {
        #region Constructors

        public ResolutionCalibrationMessageData(decimal readInitialPosition, decimal readFinalPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ReadInitialPosition = readInitialPosition;
            this.ReadFinalPosition = readFinalPosition;
        }

        public ResolutionCalibrationMessageData(decimal newResolution)
        {
            this.Resolution = newResolution;
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
