using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces
{
    internal interface IShutterPositioningMachineData : IMachineData
    {
        #region Properties

        InverterIndex InverterIndex { get; }

        IMachineResourcesProvider MachineSensorsStatus { get; }

        IShutterPositioningMessageData PositioningMessageData { get; }

        #endregion
    }
}
