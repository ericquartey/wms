// ReSharper disable ArrangeThisQualifier
using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.WeightAcquisition;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
{
    internal partial class MissionsManagerService
    {


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
            switch (command.Type)
            {
                case MessageType.WeightAcquisitionCommand:
                {
                    this.OnWeightAcquisitionProcedureCommandReceived(command);
                    break;
                }

                case MessageType.ChangeRunningState:
                {
                    this.OnChangeRunningStateCommandReceived(command);
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void OnActiveStateMachineCompleted(object sender, FiniteStateMachinesEventArgs eventArgs)
        {
            var stateMachine = this.activeStateMachines.FirstOrDefault(fsm => fsm.InstanceId.Equals(eventArgs.InstanceId));
            if (stateMachine != null)
            {
                if (stateMachine is IDisposable disposableMachine)
                {
                    disposableMachine.Dispose();
                }

                stateMachine.Completed -= this.OnActiveStateMachineCompleted;

                this.activeStateMachines.Remove(stateMachine);

                this.EventAggregator.GetEvent<NotificationEvent>().Publish(eventArgs.NotificationMessage);
            }
            else
            {
                var commandMessage = (sender as IFiniteStateMachine)?.StartData;
                this.NotifyCommandError(commandMessage);
            }
        }

        private void OnChangeRunningStateCommandReceived(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            switch (((ChangeRunningStateMessageData)command.Data).CommandAction)
            {
                case CommandAction.Start:
                    if (this.activeStateMachines.Any(asm => asm.GetType() == typeof(IChangeRunningStateStateMachine)))
                    {
                        this.NotifyCommandError(command);
                    }
                    else
                    {
                        var newStateMachine = this.serviceScope.ServiceProvider.GetRequiredService<IChangeRunningStateStateMachine>();
                        newStateMachine.Completed += this.OnActiveStateMachineCompleted;
                        this.activeStateMachines.Add(newStateMachine);

                        newStateMachine.Start(command, this.CancellationToken);
                    }

                    break;
            }
        }

        private void OnWeightAcquisitionProcedureCommandReceived(CommandMessage command)
        {
            if (command is null)
            {
                return;
            }

            switch (((WeightAcquisitionCommandMessageData)command.Data).CommandAction)
            {
                case CommandAction.Start:
                    if (this.activeStateMachines.Any(asm => asm.GetType() == typeof(IWeightAcquisitionStateMachine)))
                    {
                        this.NotifyCommandError(command);
                    }
                    else
                    {
                        var newStateMachine = this.serviceScope.ServiceProvider.GetRequiredService<IWeightAcquisitionStateMachine>();
                        newStateMachine.Completed += this.OnActiveStateMachineCompleted;
                        this.activeStateMachines.Add(newStateMachine);

                        newStateMachine.Start(command, this.CancellationToken);
                    }

                    break;
            }
        }

        #endregion
    }
}
