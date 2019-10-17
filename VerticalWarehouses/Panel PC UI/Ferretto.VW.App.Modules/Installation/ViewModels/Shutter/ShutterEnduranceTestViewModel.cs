using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEnduranceTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly ShutterSensors sensors;

        private readonly IMachineShuttersWebService shuttersWebService;

        private int bayNumber;

        private int? cumulativePerformedCycles;

        private int? cumulativePerformedCyclesBeforeStart;

        private int? inputDelayBetweenCycles;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private int? performedCyclesThisSession;

        private SubscriptionToken receivedActionUpdateErrorToken;

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
            if (bayManager is null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));

            this.shuttersWebService = shuttersWebService ?? throw new System.ArgumentNullException(nameof(shuttersWebService));

            this.bayManager = bayManager;

            this.sensors = new ShutterSensors(this.BayNumber);
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

            this.shutterTestStatusChangedToken?.Dispose();
            this.shutterTestStatusChangedToken = null;

            this.sensorsChangedToken?.Dispose();
            this.sensorsChangedToken = null;

            this.receivedActionUpdateErrorToken?.Dispose();
            this.receivedActionUpdateErrorToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.shutterTestStatusChangedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                    this.OnShutterTestStatusChanged,
                    ThreadOption.UIThread,
                    false,
                    message =>
                        message?.Data != null
                        &&
                        message.Type == CommonUtils.Messages.Enumerations.MessageType.ShutterPositioning);

            this.sensorsChangedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    this.OnSensorsChanged,
                    ThreadOption.UIThread,
                    false);

            try
            {
                this.IsWaitingForResponse = true;

                var bay = await this.bayManager.GetBay();
                this.BayNumber = (int)bay.Number;

                var procedureParameters = await this.shuttersWebService.GetTestParametersAsync();
                this.InputRequiredCycles = procedureParameters.RequiredCycles;
                this.InputDelayBetweenCycles = 1;
                this.CumulativePerformedCycles = procedureParameters.PerformedCycles;
                this.CumulativePerformedCyclesBeforeStart = this.CumulativePerformedCycles;

                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                this.sensors.Update(sensorsStates.ToArray());
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
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            this.sensors.Update(message.Data?.SensorsStates);
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
            else
            {
                System.Diagnostics.Debug.WriteLine($"{message.Status} {message.Data.PerformedCycles}");
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
