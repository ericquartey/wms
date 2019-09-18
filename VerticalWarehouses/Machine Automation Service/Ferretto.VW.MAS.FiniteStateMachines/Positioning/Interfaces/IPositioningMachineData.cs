using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces
{
    public interface IPositioningMachineData : IMachineData
    {


        #region Properties

        IBaysProvider BaysProvider { get; }

        InverterIndex CurrentInverterIndex { get; }

        int ExecutedSteps { get; set; }

        IMachineSensorsStatus MachineSensorStatus { get; }

        IPositioningMessageData MessageData { get; set; }

        #endregion
    }
}
