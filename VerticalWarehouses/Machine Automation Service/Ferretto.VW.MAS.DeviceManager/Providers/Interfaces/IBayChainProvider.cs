using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IBayChainProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        #endregion

        #region Methods

        void Move(HorizontalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveManual(HorizontalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
