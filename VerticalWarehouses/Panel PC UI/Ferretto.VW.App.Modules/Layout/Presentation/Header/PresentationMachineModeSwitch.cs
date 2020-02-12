using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineModeSwitch : BasePresentationViewModel, IDisposable
    {
        #region Fields

        private readonly IDialogService dialogService;

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IRegionManager regionManager;

        private HealthStatus healthStatus;

        private bool isBusy;

        private bool isDisposed;

        private bool isMachineInAutomaticMode;

        private bool isMachineInLoadUnitOperations;

        private bool isMachineInSwitchingToLoadUnitOperations;

        private bool isMachineInTestMode;

        private bool isUnknownState;

        private MachineMode machineMode;

        private MachinePowerState machinePowerState;

        #endregion

        #region Constructors

        public PresentationMachineModeSwitch(
            IRegionManager regionManager,
            IMachineModeService machineModeService,
            IMachineModeWebService machineModeWebService,
            IDialogService dialogService)
            : base(PresentationTypes.MachineMode)
        {
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

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

        public bool IsMachineInLoadUnitOperations
        {
            get => this.isMachineInLoadUnitOperations;
            set => this.SetProperty(ref this.isMachineInLoadUnitOperations, value);
        }

        public bool IsMachineInSwitchingToLoadUnitOperations
        {
            get => this.isMachineInSwitchingToLoadUnitOperations;
            set => this.SetProperty(ref this.isMachineInSwitchingToLoadUnitOperations, value);
        }

        public bool IsMachineInTestMode
        {
            get => this.isMachineInTestMode;
            set => this.SetProperty(ref this.isMachineInTestMode, value);
        }

        public bool IsUnknownState
        {
            get => this.isUnknownState;
            set => this.SetProperty(ref this.isUnknownState, value, this.RaiseCanExecuteChanged);
        }

        private HealthStatus HealthStatus
        {
            get => this.healthStatus;
            set => this.SetProperty(ref this.healthStatus, value, this.OnHealthStatusPropertyChanged);
        }

        private MachineMode MachineMode
        {
            get => this.machineMode;
            set => this.SetProperty(ref this.machineMode, value, this.OnMachineModePropertyChanged);
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
            else if (this.machineMode is MachineMode.Manual || this.machineMode is MachineMode.Test)
            {
                var view = this.GetActiveView();
                if (view.Equals("LoadingUnitFromBayToBayView", StringComparison.InvariantCultureIgnoreCase) ||
                    view.Equals("LoadingUnitFromBayToCellView", StringComparison.InvariantCultureIgnoreCase) ||
                    view.Equals("LoadingUnitFromCellToBayView", StringComparison.InvariantCultureIgnoreCase) ||
                    view.Equals("LoadingUnitFromCellToCellView", StringComparison.InvariantCultureIgnoreCase))
                {
                    await this.machineModeWebService.SetLoadUnitOperationsAsync();
                }
                else
                {
                    var messageBoxResult = this.dialogService.ShowMessage(General.ConfirmMachineModeSwitchAutomatic, General.Automatic, DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        await this.machineModeWebService.SetAutomaticAsync();
                    }
                }
            }
            else if (this.machineMode is MachineMode.LoadUnitOperations)
            {
                //var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmationOperation, General.LoadUnitOperations, DialogType.Question, DialogButtons.YesNo);
                //if (messageBoxResult == DialogResult.Yes)
                //{
                //    await this.machineModeWebService.SetLoadUnitOperationsAsync();
                //}
                await this.machineModeWebService.SetManualAsync();
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
                 this.MachineMode is MachineMode.Manual
                 ||
                 this.MachineMode is MachineMode.Test
                 ||
                 this.MachineMode is MachineMode.LoadUnitOperations)
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

        private string GetActiveView()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();
            return activeView?.GetType()?.Name;
        }

        private void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            this.HealthStatus = e.HealthMasStatus;
        }

        private void OnHealthStatusPropertyChanged()
        {
            this.IsUnknownState =
                this.HealthStatus != HealthStatus.Healthy
                &&
                this.HealthStatus != HealthStatus.Degraded;
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.MachineMode = e.MachineMode;
        }

        private void OnMachineModePropertyChanged()
        {
            this.IsMachineInAutomaticMode = this.MachineMode is MachineMode.Automatic;
            this.IsMachineInTestMode = this.MachineMode is MachineMode.Test;
            this.IsMachineInLoadUnitOperations = this.MachineMode is MachineMode.LoadUnitOperations;
            this.IsMachineInSwitchingToLoadUnitOperations = this.MachineMode is MachineMode.SwitchingToLoadUnitOperations;

            this.IsBusy =
                this.MachineMode is MachineMode.SwitchingToLoadUnitOperations
                ||
                this.MachineMode is MachineMode.SwitchingToAutomatic
                ||
                this.MachineMode is MachineMode.SwitchingToManual
                ||
                this.MachineMode is MachineMode.Test;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.MachinePowerState = e.MachinePowerState;
        }

        #endregion
    }
}
