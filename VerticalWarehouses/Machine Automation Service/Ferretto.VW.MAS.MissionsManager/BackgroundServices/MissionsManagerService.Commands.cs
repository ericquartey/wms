using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.PowerEnable;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.WeightAcquisition;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
{
    internal partial class MissionsManagerService
    {


        #region Properties

        private IFiniteStateMachine ActiveStateMachineX
        {
            get => this.activeStateMachine;
            set
            {
                if(this.activeStateMachine != value)
                {
                    if(this.activeStateMachine != null)
                    {
                        this.activeStateMachine.Completed -= this.OnActiveStateMachineCompleted;
                    }

                    if(this.activeStateMachine is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    this.activeStateMachine = value;
                    if(this.activeStateMachine != null)
                    {
                        this.activeStateMachine.Completed += this.OnActiveStateMachineCompleted;
                    }
                }
            }
        }

        #endregion



        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination == MessageActor.MissionsManager
                ||
                command.Destination == MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            switch(command.Type)
            {
                case MessageType.WeightAcquisitionCommand:
                    {
                        this.OnWeightAcquisitionProcedureCommandReceived(
                            command.Data as WeightAcquisitionCommandMessageData);
                        break;
                    }

                case MessageType.PowerEnable:
                    {
                        this.OnPowerEnableCommandReceived(command.Data as PowerEnableMessageData);
                        break;
                    }
            }

            return Task.CompletedTask;
        }

        private void OnActiveStateMachineCompleted(object sender, FiniteStateMachinesEventArgs eventArgs)
        {
            var stateMachine = this.activeStateMachines.FirstOrDefault(fsm => fsm.InstanceId.Equals(eventArgs.InstanceId));
            if(stateMachine != null)
            {
                this.activeStateMachines.Remove(stateMachine);
            }
            else
            {
                var errorData = new MmExceptionMessageData(new ArgumentNullException("staeMachine"),
                    "Completion event received from detached FSM", 0);
                this.NotifyError(errorData);
            }
        }

        private void OnPowerEnableCommandReceived(PowerEnableMessageData data)
        {
            if(data is null)
            {
                return;
            }

            if(data.CommandAction == CommandAction.Start && this.ActiveStateMachine == null)
            {
                this.ActiveStateMachine = ServiceProviderServiceExtensions.GetRequiredService<IPowerEnableStateMachine>(this.serviceScope.ServiceProvider);
                this.ActiveStateMachine.Start(data, this.CancellationToken);
            }
        }

        private void OnWeightAcquisitionProcedureCommandReceived(
                    WeightAcquisitionCommandMessageData data)
        {
            if(data is null)
            {
                return;
            }

            // TODO we shall create a FSM manager to handle multiple concurrent requests
            if(data.CommandAction == CommandAction.Start && this.ActiveStateMachine == null)
            {
                this.ActiveStateMachine = ServiceProviderServiceExtensions.GetRequiredService<IWeightAcquisitionStateMachine>(this.serviceScope.ServiceProvider);
                this.ActiveStateMachine.Start(data, this.CancellationToken);
            }
        }

        #endregion
    }
}
