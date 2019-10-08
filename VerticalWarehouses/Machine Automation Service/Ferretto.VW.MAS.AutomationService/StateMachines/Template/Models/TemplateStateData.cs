using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.AutomationService.StateMachines.Template.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Template.Models
{
    public class TemplateStateData : ITemplateStateData
    {


        #region Constructors

        public TemplateStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;
            this.StopRequestReason = StopRequestReason.NoReason;

            this.Message = "Template State Data";
        }

        #endregion



        #region Properties

        public IMachineData MachineData { get; }

        public string Message { get; }

        public NotificationMessage NotificationMessage { get; set; }

        public IStateMachine ParentMachine { get; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
