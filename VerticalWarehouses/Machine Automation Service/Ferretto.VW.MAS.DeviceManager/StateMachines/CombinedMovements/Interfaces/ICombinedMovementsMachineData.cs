using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces
{
    internal interface ICombinedMovementsMachineData : IMachineData
    {
        #region Properties

        IBaysDataProvider BaysDataProvider { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        ICombinedMovementsMessageData MessageData { get; set; }

        bool OnHorizontalPositioningError { get; set; }

        bool OnHorizontalPositioningStopped { get; set; }

        bool OnVerticalPositioningError { get; set; }

        bool OnVerticalPositioningStopped { get; set; }

        MessageActor Requester { get; }

        #endregion
    }
}
