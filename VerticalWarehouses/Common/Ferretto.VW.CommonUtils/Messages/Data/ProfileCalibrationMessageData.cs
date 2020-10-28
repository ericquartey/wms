using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ProfileCalibrationMessageData : IProfileCalibrationMessageData
    {
        #region Constructors

        public ProfileCalibrationMessageData()
        {
        }

        public ProfileCalibrationMessageData(double profileStartDistance, double profileCalibrateDistance, double measured, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ProfileStartDistance = profileStartDistance;
            this.ProfileCalibrateDistance = profileCalibrateDistance;
            this.Measured = measured;

            this.Verbosity = verbosity;
        }

        public ProfileCalibrationMessageData(IProfileCalibrationMessageData ms)
        {
            this.ProfileStartDistance = ms.ProfileStartDistance;
            this.ProfileCalibrateDistance = ms.ProfileCalibrateDistance;
            this.Measured = ms.Measured;

            this.Verbosity = ms.Verbosity;
        }

        #endregion

        #region Properties

        public double Measured { get; set; }

        public double ProfileCalibrateDistance { get; set; }

        public double ProfileStartDistance { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"ProfileStartDistance:{this.ProfileStartDistance} ProfileCalibrateDistance:{this.ProfileCalibrateDistance} Measured:{this.Measured} ";
        }

        #endregion
    }
}
