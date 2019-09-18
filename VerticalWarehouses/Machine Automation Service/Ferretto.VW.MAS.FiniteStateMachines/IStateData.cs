using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interface
{
    public interface IStateData
    {


        #region Properties

        FieldNotificationMessage FieldMessage { get; set; }

        IMachineData MachineData { get; }

        IStateMachine ParentMachine { get; }

        StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
