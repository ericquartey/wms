using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal interface IStateData
    {
        #region Properties

        FieldNotificationMessage FieldMessage { get; set; }

        IMachineData MachineData { get; }

        IStateMachine ParentMachine { get; }

        StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
