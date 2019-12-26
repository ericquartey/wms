using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class DiagnosticDetailsViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private string currentMachineStatusFSM;

        private string currentMachineStatusInverter;

        private string currentMachineStatusIODriver;

        private string currentStateFSM;

        private string currentStateInverter;

        private string currentStateIODriver;

        private ICommand showDevicesCommand;

        private SubscriptionToken updateMachneStateActive;

        private SubscriptionToken updateStateActive;

        #endregion

        #region Constructors

        public DiagnosticDetailsViewModel(
            IEventAggregator eventAggregator,
            IDialogService dialogService)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.dialogService = dialogService ?? throw new System.ArgumentNullException(nameof(dialogService));
            this.NavigationViewModel = null;
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

        public ICommand ShowDevicesCommand =>
                        this.showDevicesCommand
                        ??
                        (this.showDevicesCommand = new DelegateCommand(
                            this.ShowDevices));

        #endregion

        #region Methods

        public override void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public override async Task OnEnterViewAsync()
        {
            await base.OnEnterViewAsync();

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

        public override void UnSubscribeMethodFromEvent()
        {
            base.UnSubscribeMethodFromEvent();

            this.eventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>().Unsubscribe(this.updateMachneStateActive);
            this.eventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>().Unsubscribe(this.updateStateActive);
            this.IsOpen = false;
        }

        private void ShowDevices()
        {
            this.dialogService.Show(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Devices.DEVICES);
        }

        private void UpdateMachneStateActive(MessageActor messageActor, string messageType)
        {
            switch (messageActor)
            {
                case MessageActor.DeviceManager:
                    this.CurrentMachineStatusFSM = messageType;
                    break;

                case MessageActor.InverterDriver:
                    this.CurrentMachineStatusInverter = messageType;
                    break;

                case MessageActor.IoDriver:
                    this.CurrentMachineStatusIODriver = messageType;
                    break;

                default:
                    break;
            }
        }

        private void UpdateStateActive(MessageActor messageActor, string currentState)
        {
            switch (messageActor)
            {
                case MessageActor.DeviceManager:
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
