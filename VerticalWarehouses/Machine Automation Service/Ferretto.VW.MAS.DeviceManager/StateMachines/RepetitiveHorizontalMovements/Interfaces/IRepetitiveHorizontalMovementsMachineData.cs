using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces
{
    internal interface IRepetitiveHorizontalMovementsMachineData : IMachineData
    {
        #region Properties

        bool AcquiredWeight { get; set; }

        IBaysDataProvider BaysDataProvider { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        IRepetitiveHorizontalMovementsMessageData MessageData { get; set; }

        int NPerformedCycles { get; set; }

        MessageActor Requester { get; }

        #endregion
    }
}
