using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IShutterProvider
    {
        #region Methods

        void Move(ShutterMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        bool MoveTo(ShutterPosition targetPosition, BayNumber bayNumber, MessageActor sender);

        void RunTest(int delayInSeconds, int testCycleCount, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
