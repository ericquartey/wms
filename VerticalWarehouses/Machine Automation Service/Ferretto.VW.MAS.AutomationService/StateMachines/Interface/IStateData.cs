using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Interface
{
    public interface IStateData
    {


        #region Properties

        IMachineData MachineData { get; }

        NotificationMessage NotificationMessage { get; set; }

        IStateMachine ParentMachine { get; }

        StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
