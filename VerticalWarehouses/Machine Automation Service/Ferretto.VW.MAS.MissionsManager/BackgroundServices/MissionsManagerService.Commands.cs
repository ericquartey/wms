using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager
{
    internal partial class MissionsManagerService
    {
        #region Fields

        private IFiniteStateMachine activeStateMachine;

        #endregion

        #region Properties

        private IFiniteStateMachine ActiveStateMachine
        {
            get => this.activeStateMachine;
            set
            {
                if (this.activeStateMachine != value)
                {
                    if (this.activeStateMachine != null)
                    {
                        this.activeStateMachine.Completed -= this.OnActiveStateMachineCompleted;
                    }

                    if (this.activeStateMachine is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    this.activeStateMachine = value;
                    if (this.activeStateMachine != null)
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

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            switch (command.Type)
            {
                case MessageType.PowerEnable:
                    {
                        this.OnPowerEnableCommandReceived(command.Data as PowerEnableMessageData);
                        break;
                    }
            }

            return Task.CompletedTask;
        }

        private void OnActiveStateMachineCompleted(object sender, EventArgs e)
        {
            this.ActiveStateMachine = null;
        }

        private void OnPowerEnableCommandReceived(PowerEnableMessageData data)
        {
            if (data is null)
            {
                return;
            }

            if (data.CommandAction == CommandAction.Start && this.ActiveStateMachine == null)
            {
                this.ActiveStateMachine = this.serviceScope.ServiceProvider.GetRequiredService<IChangePowerStatusStateMachine>();
                this.ActiveStateMachine.Start(data, this.CancellationToken);
            }
        }

        #endregion
    }
}
