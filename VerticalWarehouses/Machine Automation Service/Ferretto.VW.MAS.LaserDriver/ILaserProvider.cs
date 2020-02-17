using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.LaserDriver
{
    public interface ILaserProvider
    {
        #region Methods

        void MoveToPositionAndSwitchOn(BayNumber bayNumber, double x, double y);

        void SwitchOff(BayNumber bayNumber);

        void SwitchOn(BayNumber bayNumber);

        #endregion
    }
}
