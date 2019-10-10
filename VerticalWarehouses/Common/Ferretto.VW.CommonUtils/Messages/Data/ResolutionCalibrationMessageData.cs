using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ResolutionCalibrationMessageData : IResolutionCalibrationMessageData
    {
        #region Constructors

        public ResolutionCalibrationMessageData(double readInitialPosition, double readFinalPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MeasuredInitialPosition = readInitialPosition;
            this.ReadFinalPosition = readFinalPosition;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public double MeasuredInitialPosition { get; }

        public double ReadFinalPosition { get; }

        public decimal Resolution { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"FinalPosition:{this.ReadFinalPosition} InitialPosition:{this.MeasuredInitialPosition} Resolution:{this.Resolution}";
        }

        #endregion
    }
}
