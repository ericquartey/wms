using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ICarouselProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        #endregion

        #region Methods

        void Homing(Calibration calibration, BayNumber bayNumber, MessageActor sender);

        void Move(HorizontalMovementDirection direction, int? loadingUnitId, BayNumber bayNumber, MessageActor sender);

        void MoveManual(HorizontalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
