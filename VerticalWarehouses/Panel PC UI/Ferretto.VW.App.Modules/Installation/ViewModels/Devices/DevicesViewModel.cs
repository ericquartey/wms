using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class DevicesViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly List<DeviceBase> devices;

        private readonly IMachineDevicesService machineDevicesServices = ServiceLocator.Current.GetInstance<IMachineDevicesService>();

        private string currentMachineStatusFSM;

        private string currentMachineStatusInverter;

        private string currentMachineStatusIODriver;

        private string currentStateFSM;

        private string currentStateInverter;

        private string currentStateIODriver;

        private bool isBusy;

        private ICommand refreshCommand;

        private SubscriptionToken updateMachneStateActive;

        private SubscriptionToken updateStateActive;

        #endregion

        #region Constructors

        public DevicesViewModel()
        {
            this.devices = new List<DeviceBase>();
        }

        #endregion

        #region Properties

        public string CurrentMachineStatusFSM { get => this.currentMachineStatusFSM; set => this.SetProperty(ref this.currentMachineStatusFSM, value); }

        public string CurrentMachineStatusInverter { get => this.currentMachineStatusInverter; set => this.SetProperty(ref this.currentMachineStatusInverter, value); }

        public string CurrentMachineStatusIODriver { get => this.currentMachineStatusIODriver; set => this.SetProperty(ref this.currentMachineStatusIODriver, value); }

        public string CurrentStateFSM { get => this.currentStateFSM; set => this.SetProperty(ref this.currentStateFSM, value); }

        public string CurrentStateInverter { get => this.currentStateInverter; set => this.SetProperty(ref this.currentStateInverter, value); }

        public string CurrentStateIODriver { get => this.currentStateIODriver; set => this.SetProperty(ref this.currentStateIODriver, value); }

        public IEnumerable<DeviceBase> Devices => new List<DeviceBase>(this.devices);

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsOpen { get; set; }

        public ICommand RefreshCommand =>
                this.refreshCommand
                ??
                (this.refreshCommand = new DelegateCommand(
                    async () => await this.GetDataAsync(),
                    this.CanRefresh));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
            this.EventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>().Unsubscribe(this.updateMachneStateActive);
            this.EventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>().Unsubscribe(this.updateStateActive);
            this.IsOpen = false;
        }

        public async Task GetDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.devices.Clear();
                var result = await this.machineDevicesServices.GetAllAsync();
                this.devices.AddRange(result.Item1);
                this.devices.AddRange(result.Item2);
                this.RaisePropertyChanged(nameof(this.Devices));
            }
            catch
            {
            }
            finally
            {
                this.IsBusy = false;
                ((DelegateCommand)this.refreshCommand).RaiseCanExecuteChanged();
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.updateMachneStateActive = this.EventAggregator.GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>()
                .Subscribe(
                    message => this.UpdateMachneStateActive(message.Data.MessageActor, message.Data.MessageType),
                    ThreadOption.UIThread,
                    false);

            this.updateStateActive = this.EventAggregator.GetEvent<NotificationEventUI<MachineStateActiveMessageData>>()
                .Subscribe(
                    message => this.UpdateStateActive(message.Data.MessageActor, message.Data.CurrentState),
                    ThreadOption.UIThread,
                    false);

            await this.GetDataAsync();
        }

        private bool CanRefresh()
        {
            return !this.isBusy;
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
