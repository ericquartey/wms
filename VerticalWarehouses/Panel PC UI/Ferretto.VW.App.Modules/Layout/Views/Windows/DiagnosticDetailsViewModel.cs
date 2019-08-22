using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
//using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class DiagnosticDetailsViewModel : BaseViewModel//, IDiagnosticDetailsViewModel
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 16;

        private readonly IEventAggregator eventAggregator;

        private string currentMachineStatusFSM;

        private string currentMachineStatusInverter;

        private string currentMachineStatusIODriver;

        private string currentStateFSM;

        private string currentStateInverter;

        private string currentStateIODriver;

        private SubscriptionToken updateMachneStateActive;

        private SubscriptionToken updateStateActive;

        #endregion

        #region Constructors

        public DiagnosticDetailsViewModel()
        {
        }

        public DiagnosticDetailsViewModel(
            IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            //this.OnEnterViewAsync();
        }

        #endregion

        #region Properties

        public string CurrentMachineStatusFSM { get => this.currentMachineStatusFSM; set => this.SetProperty(ref this.currentMachineStatusFSM, value); }

        public string CurrentMachineStatusInverter { get => this.currentMachineStatusInverter; set => this.SetProperty(ref this.currentMachineStatusInverter, value); }

        public string CurrentMachineStatusIODriver { get => this.currentMachineStatusIODriver; set => this.SetProperty(ref this.currentMachineStatusIODriver, value); }

        public string CurrentStateFSM { get => this.currentStateFSM; set => this.SetProperty(ref this.currentStateFSM, value); }

        public string CurrentStateInverter { get => this.currentStateInverter; set => this.SetProperty(ref this.currentStateInverter, value); }

        public string CurrentStateIODriver { get => this.currentStateIODriver; set => this.SetProperty(ref this.currentStateIODriver, value); }

        public bool IsOpen { get; set; }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateMachneStateActive = this.eventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>()
                .Subscribe(
                    message => this.UpdateMachneStateActive(message.Data.MessageActor, message.Data.MessageType),
                    ThreadOption.UIThread,
                    false);

            this.updateStateActive = this.eventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>()
                .Subscribe(
                    message => this.UpdateStateActive(message.Data.MessageActor, message.Data.CurrentState),
                    ThreadOption.UIThread,
                    false);

            this.IsOpen = true;
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>().Unsubscribe(this.updateMachneStateActive);
            this.eventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>().Unsubscribe(this.updateStateActive);
            this.IsOpen = false;
        }

        private void UpdateMachneStateActive(MessageActor messageActor, string messageType)
        {
            switch (messageActor)
            {
                case MessageActor.FiniteStateMachines:
                    this.CurrentMachineStatusFSM = messageType.ToString();
                    break;

                case MessageActor.InverterDriver:
                    this.CurrentMachineStatusInverter = messageType.ToString();
                    break;

                case MessageActor.IoDriver:
                    this.CurrentMachineStatusIODriver = messageType.ToString();
                    break;

                default:
                    break;
            }
        }

        private void UpdateStateActive(MessageActor messageActor, string currentState)
        {
            switch (messageActor)
            {
                case MessageActor.FiniteStateMachines:
                    this.CurrentStateFSM = currentState;
                    break;

                case MessageActor.InverterDriver:
                    this.CurrentStateInverter = currentState;
                    break;

                case MessageActor.IoDriver:
                    this.CurrentStateIODriver = currentState;
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
