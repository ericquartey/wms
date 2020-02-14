using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;



namespace Ferretto.VW.MAS.DeviceManager.Homing.Models
{
    internal class HomingStateData : IHomingStateData
    {
        #region Constructors

        public HomingStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;

            this.StopRequestReason = StopRequestReason.NoReason;
        }

        #endregion

        #region Properties

        public FieldNotificationMessage FieldMessage { get; set; }

        public IMachineData MachineData { get; }

        public IStateMachine ParentMachine { get; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
