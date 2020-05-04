using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DeviceManager.InverterProgramming.Models
{
    internal class InverterProgrammingStateData : IInverterProgrammingStateData
    {
        #region Constructors

        public InverterProgrammingStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;
            this.StopRequestReason = StopRequestReason.NoReason;

            this.Message = "InverterProgrammingStateData";
        }

        #endregion

        #region Properties

        public FieldNotificationMessage FieldMessage { get; set; }

        public IMachineData MachineData { get; }

        public string Message { get; }

        public IStateMachine ParentMachine { get; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
