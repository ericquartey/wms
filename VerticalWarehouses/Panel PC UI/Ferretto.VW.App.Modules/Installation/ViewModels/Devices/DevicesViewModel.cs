using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class DevicesViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly List<DeviceBase> devices;

        private readonly IMachineDevicesWebService machineDevicesServices = ServiceLocator.Current.GetInstance<IMachineDevicesWebService>();

        private string currentMachineStatusFSM;

        private string currentMachineStatusInverter;

        private string currentMachineStatusIODriver;

        private string currentStateFSM;

        private string currentStateInverter;

        private string currentStateIODriver;

        private bool isBusy;

        private ICommand refreshCommand;

        private SubscriptionToken updateMachneStateActiveToken;

        private SubscriptionToken updateStateActiveToken;

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

            this.updateMachneStateActiveToken?.Dispose();
            this.updateMachneStateActiveToken = null;

            this.updateStateActiveToken?.Dispose();
            this.updateStateActiveToken = null;

            this.IsOpen = false;
        }

        public async Task GetDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.devices.Clear();
                var deviceInfo = await this.machineDevicesServices.GetDeviceInfoAsync();
                this.devices.AddRange(deviceInfo);
                var inverterInfo = await this.machineDevicesServices.GetInverterInfoAsync();
                this.devices.AddRange(inverterInfo);
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.updateMachneStateActiveToken = this.updateMachneStateActiveToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MachineStatusActiveMessageData>>()
                    .Subscribe(
                        this.OnMachineStatusChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            this.updateStateActiveToken = this.updateStateActiveToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MachineStateActiveMessageData>>()
                    .Subscribe(
                        this.OnMachineStateChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            await this.GetDataAsync();
        }

        private bool CanRefresh()
        {
            return !this.isBusy;
        }

        private void OnMachineStateChanged(NotificationMessageUI<MachineStateActiveMessageData> message)
        {
            switch (message.Data.MessageActor)
            {
                case MessageActor.DeviceManager:
                    this.CurrentStateFSM = message.Data.CurrentState;
                    break;

                case MessageActor.InverterDriver:
                    this.CurrentStateInverter = message.Data.CurrentState;
                    break;

                case MessageActor.IoDriver:
                    this.CurrentStateIODriver = message.Data.CurrentState;
                    break;

                default:
                    break;
            }
        }

        private void OnMachineStatusChanged(NotificationMessageUI<MachineStatusActiveMessageData> message)
        {
            switch (message.Data.MessageActor)
            {
                case MessageActor.DeviceManager:
                    this.CurrentMachineStatusFSM = message.Type.ToString();
                    break;

                case MessageActor.InverterDriver:
                    this.CurrentMachineStatusInverter = message.Type.ToString();
                    break;

                case MessageActor.IoDriver:
                    this.CurrentMachineStatusIODriver = message.Type.ToString();
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
