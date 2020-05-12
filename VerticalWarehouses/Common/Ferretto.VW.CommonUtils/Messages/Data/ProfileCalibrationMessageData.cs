using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ProfileCalibrationMessageData : IProfileCalibrationMessageData
    {
        #region Constructors

        public ProfileCalibrationMessageData(double? profileStartDistance, double? profileCalibrateDistance, double? measured, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ProfileStartDistance = profileStartDistance;
            this.ProfileCalibrateDistance = profileCalibrateDistance;
            this.Measured = measured;

            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public double? Measured { get; }

        public double? ProfileCalibrateDistance { get; }

        public double? ProfileStartDistance { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"ProfileStartDistance:{this.ProfileStartDistance} ProfileCalibrateDistance:{this.ProfileCalibrateDistance} Measured:{this.Measured} ";
        }

        #endregion
    }
}
