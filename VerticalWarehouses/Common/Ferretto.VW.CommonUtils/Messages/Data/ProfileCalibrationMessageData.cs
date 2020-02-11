using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ProfileCalibrationMessageData : IProfileCalibrationMessageData
    {
        #region Constructors

        public ProfileCalibrationMessageData(double? profileCalibrateDistance, double? profileStartDistance, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ProfileCalibrateDistance = profileCalibrateDistance;
            this.ProfileStartDistance = profileStartDistance;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public double? ProfileCalibrateDistance { get; }

        public double? ProfileStartDistance { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"ProfileCalibrateDistance:{this.ProfileCalibrateDistance} ProfileStartDistance:{this.ProfileStartDistance}";
        }

        #endregion
    }
}
