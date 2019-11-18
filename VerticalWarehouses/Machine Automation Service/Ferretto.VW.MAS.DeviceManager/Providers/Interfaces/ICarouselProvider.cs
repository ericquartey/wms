using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ICarouselProvider
    {
        #region Methods

        ActionPolicy CanMove(VerticalMovementDirection direction, BayNumber bayNumber);

        double GetPosition(BayNumber bayNumber);

        void Homing(Calibration calibration, BayNumber bayNumber, MessageActor sender);

        void Move(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveManual(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
