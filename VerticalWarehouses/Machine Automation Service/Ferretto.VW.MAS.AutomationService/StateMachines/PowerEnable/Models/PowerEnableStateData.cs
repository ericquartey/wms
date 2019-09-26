using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Models
{
    public class PowerEnableStateData : IPowerEnableStateData
    {


        #region Constructors

        public PowerEnableStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;
            this.StopRequestReason = StopRequestReason.NoReason;
        }

        #endregion



        #region Properties

        public IMachineData MachineData { get; }

        public NotificationMessage NotificationMessage { get; set; }

        public IStateMachine ParentMachine { get; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
