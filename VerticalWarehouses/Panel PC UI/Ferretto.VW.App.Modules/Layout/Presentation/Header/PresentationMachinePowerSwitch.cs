using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachinePowerSwitch : BasePresentation
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineSensorsService machineSensorsService;

        private readonly SubscriptionToken sensorsSubscriptionToken;

        private readonly SubscriptionToken subscriptionToken;

        private bool emergencyButtonPressed;

        private bool isBusy;

        private bool isMachinePoweredOn;

        #endregion

        #region Constructors

        public PresentationMachinePowerSwitch(
            IMachineModeService machineModeService,
            IMachineSensorsService machineSensorsService,
            IEventAggregator eventAggregator)
            : base(PresentationTypes.MachineMarch)
        {
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.machineSensorsService = machineSensorsService ?? throw new ArgumentNullException(nameof(machineSensorsService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.sensorsSubscriptionToken = this.eventAggregator
               .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
               .Subscribe(
                   message => this.OnSensorsChanged(message?.Data?.SensorsStates),
                   ThreadOption.UIThread,
                   false);

            this.subscriptionToken = this.machineModeService.MachineModeChangedEvent
                .Subscribe(
                    this.OnMachineModeChanged,
                    ThreadOption.UIThread,
                    false);
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

        public override async Task ExecuteAsync()
        {
            var sensors = await this.machineSensorsService.GetAsync();
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
                var sensors = await this.machineSensorsService.GetAsync();
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

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.IsMachinePoweredOn = e.MachinePower == Services.Models.MachinePowerState.Powered;
            this.IsBusy = false;
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

        #endregion
    }
}
