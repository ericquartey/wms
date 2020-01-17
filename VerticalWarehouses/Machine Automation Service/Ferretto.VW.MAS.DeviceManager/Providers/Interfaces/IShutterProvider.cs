﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IShutterProvider
    {
        #region Methods

        void Move(ShutterMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        bool MoveTo(ShutterPosition targetPosition, BayNumber bayNumber, MessageActor sender);

        void ResetTest(BayNumber bayNumber);

        void RunTest(int delayInSeconds, int testCycleCount, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
