using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ResolutionCalibrationMessageData : IResolutionCalibrationMessageData
    {
        #region Constructors

        public ResolutionCalibrationMessageData()
        {
        }

        public ResolutionCalibrationMessageData(double readStartPosition, double readInitialPosition, double readFinalPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ReadStartPosition = readStartPosition;
            this.MeasuredInitialPosition = readInitialPosition;
            this.ReadFinalPosition = readFinalPosition;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public double MeasuredInitialPosition { get; }

        public double ReadFinalPosition { get; }

        public double ReadStartPosition { get; }

        public double Resolution { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"StartPosition:{this.ReadStartPosition} InitialPosition:{this.MeasuredInitialPosition} FinalPosition:{this.ReadFinalPosition} Resolution:{this.Resolution}";
        }

        #endregion
    }
}
