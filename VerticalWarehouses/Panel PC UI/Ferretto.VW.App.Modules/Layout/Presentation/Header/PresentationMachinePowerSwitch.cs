using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachinePowerSwitch : BasePresentationViewModel, IDisposable
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly IMachineModeService machineModeService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachineService machineService;

        private bool isBusy;

        private bool isDisposed;

        private bool isMachinePoweredOn;

        private bool isUnknownState;

        #endregion

        #region Constructors

        public PresentationMachinePowerSwitch(
            IMachineModeService machineModeService,
            IDialogService dialogService,
            IMachineService machineService)
            : base(PresentationTypes.MachineMarch)
        {
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));

            this.machinePowerChangedToken = this.EventAggregator
                .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                .Subscribe(
                    this.OnMachinePowerChanged,
                    ThreadOption.UIThread,
                    false);

            this.healthStatusChangedToken = this.EventAggregator
                .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                .Subscribe(
                    this.OnHealthStatusChanged,
                    ThreadOption.UIThread,
                    false);

            this.UpdatePowerState(this.machineModeService.MachinePower);
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachinePoweredOn
        {
            get => this.isMachinePoweredOn;
            set => this.SetProperty(ref this.isMachinePoweredOn, value, this.RaiseCanExecuteChanged);
        }

        public bool IsUnknownState
        {
            get => this.isUnknownState;
            set => this.SetProperty(ref this.isUnknownState, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override async Task ExecuteAsync()
        {
            if (this.IsMachinePoweredOn)
            {
                await this.machineModeService.PowerOffAsync();
            }
            else
            {
                var messageBoxResult = this.dialogService.ShowMessage(Resources.Localized.Get("General.ConfirmMachineRun"), Resources.Localized.Get("General.MachineRun"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineModeService.PowerOnAsync();
                }
            }
        }

        protected override bool CanExecute()
        {
            return
                !this.isBusy
                &&
                !this.isUnknownState;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.UpdatePowerState(this.machineModeService.MachinePower);
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
                this.healthStatusChangedToken.Dispose();
            }

            this.isDisposed = true;
        }

        private void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            this.IsUnknownState =
                e.HealthMasStatus != HealthStatus.Healthy
                &&
                e.HealthMasStatus != HealthStatus.Degraded;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.UpdatePowerState(e.MachinePowerState);
        }

        private void UpdatePowerState(MachinePowerState machinePowerState)
        {
            this.IsMachinePoweredOn = machinePowerState == MachinePowerState.Powered;

            this.IsBusy =
                machinePowerState == MachinePowerState.PoweringDown
                ||
                machinePowerState == MachinePowerState.PoweringUp;
        }

        #endregion
    }
}
