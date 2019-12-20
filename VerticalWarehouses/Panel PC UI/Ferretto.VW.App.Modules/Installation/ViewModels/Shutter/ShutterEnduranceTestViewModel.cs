using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
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

        private readonly IBayManager bayManager;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly ShutterSensors sensors = new ShutterSensors();

        private readonly IMachineShuttersWebService shuttersWebService;

        private int bayNumber;

        private int? cumulativePerformedCycles;

        private int? cumulativePerformedCyclesBeforeStart;

        private int? inputDelayBetweenCycles;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isShutterThreeSensors;

        private bool isWaitingForResponse;

        private int? performedCyclesThisSession;

        private SubscriptionToken sensorsChangedToken;

        private SubscriptionToken shutterTestStatusChangedToken;

        private DelegateCommand startTestCommand;

        private DelegateCommand stopTestCommand;

        #endregion

        #region Constructors

        public ShutterEnduranceTestViewModel(
            IMachineShuttersWebService shuttersWebService,
            IBayManager bayManager,
            IMachineSensorsWebService machineSensorsWebService)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            this.shuttersWebService = shuttersWebService ?? throw new System.ArgumentNullException(nameof(shuttersWebService));
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            private set => this.SetProperty(ref this.bayNumber, value);
        }

        public int? CumulativePerformedCycles
        {
            get => this.cumulativePerformedCycles;
            private set
            {
                if (this.SetProperty(ref this.cumulativePerformedCycles, value))
                {
                    this.PerformedCyclesThisSession = this.CumulativePerformedCycles - this.CumulativePerformedCyclesBeforeStart;
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
                    this.PerformedCyclesThisSession = this.CumulativePerformedCycles - this.CumulativePerformedCyclesBeforeStart;
                }
            }
        }

        public string Error => string.Join(
            System.Environment.NewLine,
            this[nameof(this.InputDelayBetweenCycles)],
            this[nameof(this.InputRequiredCycles)]);

        public int? InputDelayBetweenCycles
        {
            get => this.inputDelayBetweenCycles;
            set
            {
                if (this.SetProperty(ref this.inputDelayBetweenCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputRequiredCycles
        {
            get => this.inputRequiredCycles;
            set
            {
                if (this.SetProperty(ref this.inputRequiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsShutterThreeSensors
        {
            get => this.isShutterThreeSensors;
            set => this.SetProperty(ref this.isShutterThreeSensors, value);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? PerformedCyclesThisSession
        {
            get => this.performedCyclesThisSession;
            private set => this.SetProperty(ref this.performedCyclesThisSession, value);
        }

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
                    case nameof(this.InputRequiredCycles):
                        if (!this.InputRequiredCycles.HasValue)
                        {
                            return $"InputRequiredCycles is required.";
                        }

                        if (this.InputRequiredCycles.Value <= 0)
                        {
                            return "InputRequiredCycles must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputDelayBetweenCycles):
                        if (!this.InputDelayBetweenCycles.HasValue)
                        {
                            return $"InputDelayBetweenCycles is required.";
                        }

                        if (this.InputDelayBetweenCycles.Value <= 0)
                        {
                            return "InputDelayBetweenCycles must be strictly positive.";
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
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.shutterTestStatusChangedToken?.Dispose();
            this.shutterTestStatusChangedToken = null;

            this.sensorsChangedToken?.Dispose();
            this.sensorsChangedToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

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

            try
            {
                this.IsWaitingForResponse = true;

                var bay = await this.bayManager.GetBayAsync();
                this.BayNumber = (int)bay.Number;

                var procedureParameters = await this.shuttersWebService.GetTestParametersAsync();
                this.InputRequiredCycles = procedureParameters.RequiredCycles;
                this.InputDelayBetweenCycles = 1;
                this.CumulativePerformedCycles = procedureParameters.PerformedCycles;

                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                this.sensors.Update(sensorsStates.ToArray(), this.BayNumber);

                this.IsShutterThreeSensors = this.MachineService.IsShutterThreeSensors;

                this.RaisePropertyChanged(nameof(this.Sensors));
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
        }

        private void OnShutterTestStatusChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message.IsErrored())
            {
                this.IsExecutingProcedure = false;

                this.ShowNotification(VW.App.Resources.InstallationApp.ProcedureWasStopped, NotificationSeverity.Warning);
            }
            else if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }
            else if (message.Data != null)
            {
                this.CumulativePerformedCycles = message.Data.PerformedCycles;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startTestCommand.RaiseCanExecuteChanged();
            this.stopTestCommand.RaiseCanExecuteChanged();
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
            catch (System.Exception ex)
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

                await this.shuttersWebService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
