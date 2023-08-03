using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ShutterEnduranceTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly ShutterSensors sensors = new ShutterSensors();

        private readonly IMachineShuttersWebService shuttersWebService;

        private int? cumulativePerformedCycles;

        private int? cumulativePerformedCyclesBeforeStart;

        private double? cyclesPercent;

        private int? inputDelayBetweenCycles;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isShutterThreeSensors;

        private int performedCyclesThisSession;

        private DelegateCommand resetTestCommand;

        private SubscriptionToken sensorsChangedToken;

        private SubscriptionToken shutterTestStatusChangedToken;

        private DelegateCommand startTestCommand;

        private DelegateCommand stopTestCommand;

        #endregion

        #region Constructors

        public ShutterEnduranceTestViewModel(
            IMachineShuttersWebService shuttersWebService,
            IMachineSensorsWebService machineSensorsWebService,
            Services.IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new ArgumentNullException(nameof(machineSensorsWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => (int)this.MachineService?.BayNumber;
        }

        public int? CumulativePerformedCycles
        {
            get => this.cumulativePerformedCycles;
            private set
            {
                if (this.SetProperty(ref this.cumulativePerformedCycles, value))
                {
                    if (this.CumulativePerformedCycles != null && this.CumulativePerformedCyclesBeforeStart != null)
                    {
                        this.PerformedCyclesThisSession = this.CumulativePerformedCycles.Value - this.CumulativePerformedCyclesBeforeStart.Value;
                    }
                }
            }
        }

        public int? CumulativePerformedCyclesBeforeStart
        {
            get => this.cumulativePerformedCyclesBeforeStart;
            private set
            {
                if (this.SetProperty(ref this.cumulativePerformedCyclesBeforeStart, value))
                {
                    if (this.CumulativePerformedCycles != null && this.CumulativePerformedCyclesBeforeStart != null)
                    {
                        this.PerformedCyclesThisSession = this.CumulativePerformedCycles.Value - this.CumulativePerformedCyclesBeforeStart.Value;
                    }
                }
            }
        }

        public double? CyclesPercent
        {
            get => this.cyclesPercent;
            private set => this.SetProperty(ref this.cyclesPercent, value);
        }

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputDelayBetweenCycles)]);

        public int? InputDelayBetweenCycles
        {
            get => this.inputDelayBetweenCycles;
            set => this.SetProperty(ref this.inputDelayBetweenCycles, value);
        }

        public int? InputRequiredCycles
        {
            get => this.inputRequiredCycles;
            set
            {
                this.SetProperty(ref this.inputRequiredCycles, value);
                this.shuttersWebService.SetBayShutterRequiredCyclesAsync(value.Value);
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public bool IsShutterThreeSensors
        {
            get => this.isShutterThreeSensors;
            set => this.SetProperty(ref this.isShutterThreeSensors, value);
        }

        public int PerformedCyclesThisSession
        {
            get => this.performedCyclesThisSession;
            private set => this.SetProperty(ref this.performedCyclesThisSession, value);
        }

        public ICommand ResetTestCommand =>
            this.resetTestCommand
            ??
            (this.resetTestCommand = new DelegateCommand(
                async () => await this.ResetTestAsync(),
                this.CanExecuteResetTestCommand));

        public ShutterSensors Sensors => this.sensors;

        public ICommand StartCommand =>
            this.startTestCommand
            ??
            (this.startTestCommand = new DelegateCommand(
                async () => await this.StartTestAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopTestCommand
            ??
            (this.stopTestCommand = new DelegateCommand(
                async () => await this.StopTestAsync(),
                this.CanExecuteStopCommand));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputDelayBetweenCycles):
                        if (!this.InputDelayBetweenCycles.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputDelayBetweenCyclesRequired");
                        }

                        if (this.InputDelayBetweenCycles.Value < 0)
                        {
                            return Localized.Get("InstallationApp.InputDelayBetweenCyclesMustBePositive");
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
            if (this.shutterTestStatusChangedToken != null)
            {
                this.EventAggregator?.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Unsubscribe(this.shutterTestStatusChangedToken);
                this.shutterTestStatusChangedToken?.Dispose();
                this.shutterTestStatusChangedToken = null;
            }

            if (this.sensorsChangedToken != null)
            {
                this.EventAggregator?.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.sensorsChangedToken);
                this.sensorsChangedToken?.Dispose();
                this.sensorsChangedToken = null;
            }
            */
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            var procedureParameters = await this.shuttersWebService.GetTestParametersAsync();
            this.InputRequiredCycles = procedureParameters.RequiredCycles;
            if (!this.InputDelayBetweenCycles.HasValue)
            {
                this.InputDelayBetweenCycles = 1;
            }

            this.CumulativePerformedCycles = procedureParameters.PerformedCycles;

            var sensorsStates = await this.machineSensorsWebService.GetAsync();
            this.sensors.Update(sensorsStates.ToArray(), this.BayNumber);

            this.IsShutterThreeSensors = this.MachineService.IsShutterThreeSensors;

            this.RaisePropertyChanged(nameof(this.Sensors));
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startTestCommand?.RaiseCanExecuteChanged();
            this.stopTestCommand?.RaiseCanExecuteChanged();
            this.resetTestCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteResetTestCommand()
        {
            return this.CumulativePerformedCycles.HasValue &&
                   this.CumulativePerformedCycles > 0 &&
                   !this.IsExecutingProcedure &&
                   string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStartCommand()
        {
            return this.CumulativePerformedCycles.HasValue &&
                   this.InputRequiredCycles.HasValue &&
                   this.CumulativePerformedCycles < this.InputRequiredCycles &&
                   !this.IsExecutingProcedure &&
                   !this.MachineStatus.IsMovingShutter &&
                   string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return this.MachineStatus.IsMovingShutter &&
                   this.IsExecutingProcedure;
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStatesInput);
        }

        private void OnShutterTestStatusChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message.IsErrored())
            {
                this.IsExecutingProcedure = false;

                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.ProcedureWasStopped"), NotificationSeverity.Warning);
            }
            else if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }

            if (message.Data != null)
            {
                this.CumulativePerformedCycles = message.Data.PerformedCycles;

                this.CyclesPercent = ((double)this.PerformedCyclesThisSession / (double)this.InputRequiredCycles) * 100.0;
            }
        }

        private async Task ResetTestAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.ShutterEnduranceTest"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.CumulativePerformedCycles = 0;
                    this.CumulativePerformedCyclesBeforeStart = 0;

                    await this.shuttersWebService.ResetTestAsync();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StartTestAsync()
        {
            try
            {
                this.IsExecutingProcedure = true;
                this.IsWaitingForResponse = true;

                this.CumulativePerformedCyclesBeforeStart = this.CumulativePerformedCycles;

                await this.shuttersWebService.RunTestAsync(
                    this.InputDelayBetweenCycles.Value,
                    this.InputRequiredCycles.Value);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopTestAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.MachineService.StopMovingByAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        private void SubscribeToEvents()
        {
            this.shutterTestStatusChangedToken = this.shutterTestStatusChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Subscribe(
                        this.OnShutterTestStatusChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Type == CommonUtils.Messages.Enumerations.MessageType.ShutterPositioning);

            this.sensorsChangedToken = this.sensorsChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);
        }

        #endregion
    }
}
