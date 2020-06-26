using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements.Models
{
    internal class CombinedMovementsMachineData : ICombinedMovementsMachineData
    {
        #region Properties

        public IBaysDataProvider BaysDataProvider => throw new NotImplementedException();

        public int ExecutedSteps { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IMachineResourcesProvider MachineSensorStatus => throw new NotImplementedException();

        public ICombinedMovementsMessageData MessageData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public MessageActor Requester => throw new NotImplementedException();

        #endregion
    }
}
