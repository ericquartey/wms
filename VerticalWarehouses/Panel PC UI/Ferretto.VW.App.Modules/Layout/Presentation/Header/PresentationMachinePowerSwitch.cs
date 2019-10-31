using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachinePowerSwitch : BasePresentationViewModel, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineModeService machineModeService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly SubscriptionToken sensorsChangedSubscriptionToken;

        private bool emergencyButtonPressed;

        private bool isBusy;

        private bool isDisposed;

        private bool isMachinePoweredOn;

        #endregion

        #region Constructors

        public PresentationMachinePowerSwitch(
            IMachineModeService machineModeService,
            IMachineSensorsWebService machineSensorsWebService,
            IEventAggregator eventAggregator)
            : base(PresentationTypes.MachineMarch)
        {
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.sensorsChangedSubscriptionToken = this.eventAggregator
               .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
               .Subscribe(
                   message => this.OnSensorsChanged(message?.Data?.SensorsStates),
                   ThreadOption.UIThread,
                   false);

            this.machinePowerChangedToken = this.EventAggregator
                .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                .Subscribe(
                    this.OnMachinePowerChanged,
                    ThreadOption.UIThread,
                    false);

            this.UpdatePowerState(this.machineModeService.MachinePower);
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMachinePoweredOn
        {
            get => this.isMachinePoweredOn;
            set => this.SetProperty(ref this.isMachinePoweredOn, value);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override async Task ExecuteAsync()
        {
            var sensors = await this.machineSensorsWebService.GetAsync();
            this.OnSensorsChanged(sensors.ToArray());

            if (this.emergencyButtonPressed)
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                dialogService.ShowMessage(Resources.VWApp.EnsureEmergencyIsOff, Resources.VWApp.EmergencyIsOn, DialogType.Exclamation, DialogButtons.OK);
                return;
            }

            this.IsBusy = true;

            if (this.IsMachinePoweredOn)
            {
                await this.machineModeService.PowerOffAsync();
            }
            else
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage("Confirmation operation?", "March", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineModeService.PowerOnAsync();
                }
                else
                {
                    this.IsBusy = false;
                }
            }
        }

        public override async Task OnLoadedAsync()
        {
            await base.OnLoadedAsync();

            try
            {
                var sensors = await this.machineSensorsWebService.GetAsync();
                this.OnSensorsChanged(sensors.ToArray());
            }
            catch
            {
            }
        }

        protected override bool CanExecute()
        {
            return !this.isBusy;
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.machinePowerChangedToken.Dispose();
                this.sensorsChangedSubscriptionToken.Dispose();
            }

            this.isDisposed = true;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                            $"####### Presentation ## OnPowerChange ## {e.MachinePowerState}");

            this.UpdatePowerState(e.MachinePowerState);
        }

        private void OnSensorsChanged(bool[] sensorsStates)
        {
            var emergencyPressed =
                sensorsStates[(int)IOMachineSensors.MushroomEmergencyButtonBay1]
                ||
                sensorsStates[(int)IOMachineSensors.MushroomEmergencyButtonBay2]
                ||
                sensorsStates[(int)IOMachineSensors.MushroomEmergencyButtonBay3];

            if (this.emergencyButtonPressed != emergencyPressed)
            {
                this.IsBusy = false;
                this.emergencyButtonPressed = emergencyPressed;
            }
        }

        private void UpdatePowerState(MachinePowerState machinePowerState)
        {
            System.Diagnostics.Debug.WriteLine(
                            $"####### Presentation {nameof(this.IsMachinePoweredOn)}={this.IsMachinePoweredOn} ## {machinePowerState}");

            this.IsMachinePoweredOn = machinePowerState == MachinePowerState.Powered;

            this.IsBusy =
                machinePowerState == MachinePowerState.PoweringDown
                ||
                machinePowerState == MachinePowerState.PoweringUp;
        }

        #endregion
    }
}
