using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Models
{
    public class TemplateStateData : ITemplateStateData
    {


        #region Constructors

        public TemplateStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;

            this.Message = "Template State Data";
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
