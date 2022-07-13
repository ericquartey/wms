using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
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

        private readonly IMachineErrorsService machineErrorsService;

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly IMachineModeService machineModeService;

        private readonly IMachineModeWebService machineModeWebService;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachineService machineService;

        private readonly SubscriptionToken machineStatusChangesToken;

        private readonly IRegionManager regionManager;

        private HealthStatus healthStatus;

        private bool isBusy;

        private bool isDisposed;

        private bool isMachineInAutomaticMode;

        private bool isMachineInCompact;

        private bool isMachineInCompact2;

        private bool isMachineInCompact3;

        private bool isMachineInFirstTest;

        private bool isMachineInLoadUnitOperations;

        private bool isMachineInLoadUnitOperations2;

        private bool isMachineInLoadUnitOperations3;

        private bool isMachineInManualMode;

        private bool isMachineInManualMode2;

        private bool isMachineInManualMode3;

        private bool isMachineInShutdown;

        private bool isMachineInSwitchingToLoadUnitOperations;

        private bool isMachineInTestMode;

        private bool isMachineInTestMode2;

        private bool isMachineInTestMode3;

        private bool isMissionInErrorByLoadUnitOperations;

        private bool isUnknownState;

        private MachineMode machineMode;

        private MachinePowerState machinePowerState;

        #endregion

        #region Constructors

        public PresentationMachineModeSwitch(
            IRegionManager regionManager,
            IMachineModeService machineModeService,
            IMachineModeWebService machineModeWebService,
            IDialogService dialogService,
            IMachineService machineService,
            IMachineErrorsService machineErrorsService)
            : base(PresentationTypes.MachineMode)
        {
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));

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

            this.machineStatusChangesToken = this.machineStatusChangesToken
                ?? this.EventAggregator
                    .GetEvent<MachineStatusChangedPubSubEvent>()
                    .Subscribe(
                        //(m) => this.IsMissionInErrorByLoadUnitOperations = this.machineService.IsMissionInErrorByLoadUnitOperations,
                        (m) => this.OnMachineStatusChanged(),
                        ThreadOption.UIThread,
                        false);

            this.MachineMode = this.machineModeService.MachineMode;
            this.MachinePowerState = this.machineModeService.MachinePower;
            this.IsMissionInErrorByLoadUnitOperations = this.machineService.IsMissionInErrorByLoadUnitOperations;
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
            set => this.SetProperty(ref this.isMachineInAutomaticMode, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInCompact
        {
            get => this.isMachineInCompact;
            set => this.SetProperty(ref this.isMachineInCompact, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInCompact2
        {
            get => this.isMachineInCompact2;
            set => this.SetProperty(ref this.isMachineInCompact2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInCompact3
        {
            get => this.isMachineInCompact3;
            set => this.SetProperty(ref this.isMachineInCompact3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInFirstTest
        {
            get => this.isMachineInFirstTest;
            set => this.SetProperty(ref this.isMachineInFirstTest, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInLoadUnitOperations
        {
            get => this.isMachineInLoadUnitOperations;
            set => this.SetProperty(ref this.isMachineInLoadUnitOperations, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInLoadUnitOperations2
        {
            get => this.isMachineInLoadUnitOperations2;
            set => this.SetProperty(ref this.isMachineInLoadUnitOperations2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInLoadUnitOperations3
        {
            get => this.isMachineInLoadUnitOperations3;
            set => this.SetProperty(ref this.isMachineInLoadUnitOperations3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInManualMode
        {
            get => this.isMachineInManualMode;
            set => this.SetProperty(ref this.isMachineInManualMode, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInManualMode2
        {
            get => this.isMachineInManualMode2;
            set => this.SetProperty(ref this.isMachineInManualMode2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInManualMode3
        {
            get => this.isMachineInManualMode3;
            set => this.SetProperty(ref this.isMachineInManualMode3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInShutdown
        {
            get => this.isMachineInShutdown;
            set => this.SetProperty(ref this.isMachineInShutdown, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInSwitchingToLoadUnitOperations
        {
            get => this.isMachineInSwitchingToLoadUnitOperations;
            set => this.SetProperty(ref this.isMachineInSwitchingToLoadUnitOperations, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInTestMode
        {
            get => this.isMachineInTestMode;
            set => this.SetProperty(ref this.isMachineInTestMode, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInTestMode2
        {
            get => this.isMachineInTestMode2;
            set => this.SetProperty(ref this.isMachineInTestMode2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMachineInTestMode3
        {
            get => this.isMachineInTestMode3;
            set => this.SetProperty(ref this.isMachineInTestMode3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMissionInErrorByLoadUnitOperations
        {
            get => this.isMissionInErrorByLoadUnitOperations;
            set => this.SetProperty(ref this.isMissionInErrorByLoadUnitOperations, value, this.RaiseCanExecuteChanged);
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
            if (this.IsMissionInErrorByLoadUnitOperations)
            {
                await this.machineModeWebService.SetAutomaticAsync();
                this.machineErrorsService.AutoNavigateOnError = true;
            }
            else if (this.machineMode is MachineMode.Automatic)
            {
                await this.machineModeWebService.SetManualAsync();
            }
            else if (this.machineService.BayNumber == BayNumber.BayTwo &&
                (this.MachineModeService.MachineMode == MachineMode.Manual2 ||
                this.machineMode is MachineMode.Test2))
            {
                if (this.machineService.IsTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Resources.Localized.Get("General.ConfirmMachineModeSwitchAutomatic"), Resources.Localized.Get("General.Automatic"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        await this.machineModeWebService.SetAutomaticAsync();
                        this.machineErrorsService.AutoNavigateOnError = true;
                    }
                }
                else
                {
                    var messageBoxResult = this.dialogService.ShowMessage("Completare tutte le procedure di setup e calibrazione prima di entrare in modalità automatica.", Resources.Localized.Get("General.MachineRun"), DialogType.Information, DialogButtons.OK);
                }
            }
            else if (this.machineService.BayNumber == BayNumber.BayThree &&
                (this.MachineModeService.MachineMode == MachineMode.Manual3 ||
                this.machineMode is MachineMode.Test3))
            {
                if (this.machineService.IsTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Resources.Localized.Get("General.ConfirmMachineModeSwitchAutomatic"), Resources.Localized.Get("General.Automatic"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        await this.machineModeWebService.SetAutomaticAsync();
                        this.machineErrorsService.AutoNavigateOnError = true;
                    }
                }
                else
                {
                    var messageBoxResult = this.dialogService.ShowMessage("Completare tutte le procedure di setup e calibrazione prima di entrare in modalità automatica.", Resources.Localized.Get("General.MachineRun"), DialogType.Information, DialogButtons.OK);
                }
            }
            else if (this.MachineModeService.MachineMode == MachineMode.Manual ||
                this.machineMode is MachineMode.Test)
            {
                if (this.machineService.IsTuningCompleted || ConfigurationManager.AppSettings.GetOverrideSetupStatus())
                {
                    var messageBoxResult = this.dialogService.ShowMessage(Resources.Localized.Get("General.ConfirmMachineModeSwitchAutomatic"), Resources.Localized.Get("General.Automatic"), DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        await this.machineModeWebService.SetAutomaticAsync();
                        this.machineErrorsService.AutoNavigateOnError = true;
                    }
                }
                else
                {
                    var messageBoxResult = this.dialogService.ShowMessage("Completare tutte le procedure di setup e calibrazione prima di entrare in modalità automatica.", Resources.Localized.Get("General.MachineRun"), DialogType.Information, DialogButtons.OK);
                }
            }
            else if (this.machineMode is MachineMode.LoadUnitOperations ||
                this.machineMode is MachineMode.LoadUnitOperations2 ||
                this.machineMode is MachineMode.LoadUnitOperations3)
            {
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
            var res =
                !this.IsBusy
                &&
                this.MachinePowerState is MachinePowerState.Powered
                &&
                !this.IsUnknownState
                &&
                (this.machineErrorsService.ActiveError == null);

            switch (this.machineService?.BayNumber)
            {
                case MAS.AutomationService.Contracts.BayNumber.BayOne:
                default:
                    return res &&
                        (this.MachineModeService.MachineMode == MachineMode.Manual ||
                        this.MachineModeService.MachineMode == MachineMode.Automatic ||
                        this.MachineModeService.MachineMode == MachineMode.Test ||
                        this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations);

                case MAS.AutomationService.Contracts.BayNumber.BayTwo:
                    return res &&
                        (this.MachineModeService.MachineMode == MachineMode.Manual2 ||
                        this.MachineModeService.MachineMode == MachineMode.Automatic ||
                        this.MachineModeService.MachineMode == MachineMode.Test2 ||
                        this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations2);

                case MAS.AutomationService.Contracts.BayNumber.BayThree:
                    return res &&
                        (this.MachineModeService.MachineMode == MachineMode.Manual3 ||
                        this.MachineModeService.MachineMode == MachineMode.Automatic ||
                        this.MachineModeService.MachineMode == MachineMode.Test3 ||
                        this.MachineModeService.MachineMode == MachineMode.LoadUnitOperations3);
            }
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
                this.machineStatusChangesToken.Dispose();
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
            this.IsMachineInManualMode = this.MachineMode is MachineMode.Manual;
            this.IsMachineInManualMode2 = this.MachineMode is MachineMode.Manual2;
            this.IsMachineInManualMode3 = this.MachineMode is MachineMode.Manual3;
            this.IsMachineInTestMode = this.MachineMode is MachineMode.Test;
            this.IsMachineInTestMode2 = this.MachineMode is MachineMode.Test2;
            this.IsMachineInTestMode3 = this.MachineMode is MachineMode.Test3;
            this.IsMachineInLoadUnitOperations = this.MachineMode is MachineMode.LoadUnitOperations;
            this.IsMachineInLoadUnitOperations2 = this.MachineMode is MachineMode.LoadUnitOperations2;
            this.IsMachineInLoadUnitOperations3 = this.MachineMode is MachineMode.LoadUnitOperations3;
            this.IsMachineInSwitchingToLoadUnitOperations = this.MachineMode is MachineMode.SwitchingToLoadUnitOperations || this.MachineMode is MachineMode.SwitchingToLoadUnitOperations2 || this.MachineMode is MachineMode.SwitchingToLoadUnitOperations3;
            this.IsMachineInCompact = this.MachineMode is MachineMode.Compact;
            this.IsMachineInCompact2 = this.MachineMode is MachineMode.Compact2;
            this.IsMachineInCompact3 = this.MachineMode is MachineMode.Compact3;
            this.IsMachineInFirstTest = this.MachineMode is MachineMode.FirstTest || this.MachineMode is MachineMode.FirstTest2 || this.MachineMode is MachineMode.FirstTest3;
            this.IsMachineInShutdown = this.MachineMode is MachineMode.Shutdown;

            this.IsMissionInErrorByLoadUnitOperations = this.machineService.IsMissionInErrorByLoadUnitOperations;

            switch (this.machineService.BayNumber)
            {
                case BayNumber.BayOne:
                    this.IsBusy =
                this.MachineMode is MachineMode.SwitchingToLoadUnitOperations
                ||
                this.MachineMode is MachineMode.SwitchingToAutomatic
                ||
                this.MachineMode is MachineMode.SwitchingToManual
                ||
                this.MachineMode is MachineMode.Test;
                    break;

                case BayNumber.BayTwo:
                    this.IsBusy =
                this.MachineMode is MachineMode.SwitchingToLoadUnitOperations2
                ||
                this.MachineMode is MachineMode.SwitchingToAutomatic
                ||
                this.MachineMode is MachineMode.SwitchingToManual2
                ||
                this.MachineMode is MachineMode.Test2;
                    break;

                case BayNumber.BayThree:
                    this.IsBusy =
                this.MachineMode is MachineMode.SwitchingToLoadUnitOperations3
                ||
                this.MachineMode is MachineMode.SwitchingToAutomatic
                ||
                this.MachineMode is MachineMode.SwitchingToManual3
                ||
                this.MachineMode is MachineMode.Test3;
                    break;

                default:
                    this.IsBusy =
                this.MachineMode is MachineMode.SwitchingToLoadUnitOperations
                ||
                this.MachineMode is MachineMode.SwitchingToAutomatic
                ||
                this.MachineMode is MachineMode.SwitchingToManual
                ||
                this.MachineMode is MachineMode.Test;
                    break;
            }
            //this.IsBusy =
            //    this.MachineMode is MachineMode.SwitchingToLoadUnitOperations
            //    ||
            //    this.MachineMode is MachineMode.SwitchingToAutomatic
            //    ||
            //    this.MachineMode is MachineMode.SwitchingToManual
            //    ||
            //    this.MachineMode is MachineMode.Test;
        }

        private void OnMachinePowerChanged(MachinePowerChangedEventArgs e)
        {
            this.MachinePowerState = e.MachinePowerState;
        }

        private void OnMachineStatusChanged()
        {
            this.IsMissionInErrorByLoadUnitOperations = this.machineService.IsMissionInErrorByLoadUnitOperations;

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
