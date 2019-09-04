using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.FiniteStateMachines.Interface
{
    public interface IStateData
    {


        #region Properties

        FieldNotificationMessage FieldMessage { get; set; }

        IMachineData MachineData { get; }

        IStateMachine ParentMachine { get; }

        bool StopRequested { get; set; }

        #endregion
    }
}
