using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineModeSwitch : BasePresentationViewModel, IDisposable
    {
        #region Fields

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly MachineModeService machineModeService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private HealthStatus healthStatus;

        private bool isBusy;

        private bool isDisposed;

        private bool isMachineInAutomaticMode;

        private bool isUnknownState;

        private MachineMode machineMode;

        private MachinePowerState machinePowerState;

        #endregion

        #region Constructors

        public PresentationMachineModeSwitch(
            MachineModeService machineModeService,
            IMachineModeWebService machineModeWebService)
            : base(PresentationTypes.MachineMode)
        {
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));

            this.machineModeChangedToken = this.EventAggregator
              .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
              .Subscribe(
                  this.OnMachineModeChanged,
                  ThreadOption.UIThread,
                  false);

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

            this.MachineMode = this.machineModeService.MachineMode;
            this.MachinePowerState = this.machineModeService.MachinePower;
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInAutomaticMode
        {
            get => this.isMachineInAutomaticMode;
            set => this.SetProperty(ref this.isMachineInAutomaticMode, value);
        }

        public bool IsUnknownState
        {
            get => this.isUnknownState;
            set => this.SetProperty(ref this.isUnknownState, value, this.RaiseCanExecuteChanged);
        }

        private HealthStatus HealthStatus
        {
            get => this.healthStatus;
            set
            {
                if (this.SetProperty(ref this.healthStatus, value))
                {
                    this.IsUnknownState =
                        this.HealthStatus != HealthStatus.Healthy
                        &&
                        this.HealthStatus != HealthStatus.Degraded;
                }
            }
        }

        private MachineMode MachineMode
        {
            get => this.machineMode;
            set
            {
                if (this.SetProperty(ref this.machineMode, value))
                {
                    this.IsMachineInAutomaticMode = this.MachineMode is MachineMode.Automatic;

                    this.IsBusy =
                        this.MachineMode is MachineMode.SwitchingToAutomatic
                        ||
                        this.MachineMode is MachineMode.SwitchingToManual;
                }
            }
        }

        private MachinePowerState MachinePowerState
        {
            get => this.machinePowerState;
            set => this.SetProperty(ref this.machinePowerState, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public override async Task ExecuteAsync()
        {
            if (this.machineMode is MachineMode.Automatic)
            {
                await this.machineModeWebService.SetManualAsync();
            }
            else if (this.machineMode is MachineMode.Manual)
            {
                await this.machineModeWebService.SetAutomaticAsync();
            }
            else
            {
                NLog.LogManager
                    .GetCurrentClassLogger()
                    .Warn($"Called Machine Mode switch command while the machine is in the {this.machineMode} state.");
            }
        }

        protected override bool CanExecute()
        {
            return
                !this.IsBusy
                &&
                (this.MachineMode is MachineMode.Automatic
                ||
                this.MachineMode is MachineMode.Manual)
                &&
                this.MachinePowerState is MachinePowerState.Powered
                &&
                !this.IsUnknownState;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.machineModeChangedToken.Dispose();
                this.machinePowerChangedToken.Dispose();
                this.healthStatusChangedToken.Dispose();
            }

            this.isDisposed = true;
        }

        private void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            this.HealthStatus = e.HealthStatus;
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.MachineMode = e.MachineMode;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.MachinePowerState = e.MachinePowerState;
        }

        #endregion
    }
}
