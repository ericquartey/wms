using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEnduranceTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineSensorsService machineSensorsService;

        private readonly ShutterSensors sensors;

        private readonly IMachineShuttersService shuttersService;

        private int bayNumber;

        private int? completedCycles;

        private int? inputDelayBetweenCycles;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private SubscriptionToken receivedActionUpdateCompletedToken;

        private SubscriptionToken receivedActionUpdateErrorToken;

        private SubscriptionToken receivedSensorsToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ShutterEnduranceTestViewModel(
            IMachineShuttersService shuttersService,
            IBayManager bayManager,
            IMachineSensorsService machineSensorsService)
            : base(PresentationMode.Installer)
        {
            if (shuttersService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersService));
            }

            if (bayManager is null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            if (machineSensorsService is null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            this.machineSensorsService = machineSensorsService;

            this.shuttersService = shuttersService;

            this.BayNumber = bayManager.Bay.Number;

            this.sensors = new ShutterSensors(this.BayNumber);
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            private set => this.SetProperty(ref this.bayNumber, value);
        }

        public int? CompletedCycles
        {
            get => this.completedCycles;
            private set => this.SetProperty(ref this.completedCycles, value);
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
                        this.ShowNotification(string.Empty, NotificationSeverity.Clear);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ShutterSensors Sensors => this.sensors;

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
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

            if (this.receivedActionUpdateCompletedToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Unsubscribe(this.receivedActionUpdateCompletedToken);

                this.receivedActionUpdateCompletedToken = null;
            }

            if (this.receivedSensorsToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.receivedSensorsToken);

                this.receivedSensorsToken = null;
            }

            if (this.receivedActionUpdateErrorToken != null)
            {
                this.EventAggregator
                 .GetEvent<MachineAutomationErrorPubSubEvent>()
                 .Unsubscribe(this.receivedActionUpdateErrorToken);

                this.receivedActionUpdateErrorToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            await this.RetrieveTestParametersAsync();

            // TODO swap between 2/3 positions bay

            this.receivedActionUpdateCompletedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                    message => this.OnShutterTestStatusChanged(message),
                    ThreadOption.UIThread,
                    false);

            this.receivedActionUpdateErrorToken = this.EventAggregator
                .GetEvent<MachineAutomationErrorPubSubEvent>()
                .Subscribe(
                    msg => this.UpdateError(),
                    ThreadOption.UIThread,
                    false,
                    message =>
                    message.NotificationType == NotificationType.Error &&
                    message.ActionType == ActionType.ShutterPositioning &&
                    message.ActionStatus == ActionStatus.Error);

            this.receivedSensorsToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.sensors.Update(message?.Data?.SensorsStates),
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

                this.sensors.Update(sensorsStates.ToArray());
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanExecuteStartCommand()
        {
            return !this.IsExecutingProcedure
                && !this.IsWaitingForResponse
                && string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return this.IsExecutingProcedure
                && !this.IsWaitingForResponse;
        }

        private async Task StartAsync()
        {
            this.IsExecutingProcedure = true;
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.RunTestAsync(
                    this.BayNumber,
                    this.InputDelayBetweenCycles.Value,
                    this.InputRequiredCycles.Value);
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

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersService.StopAsync();
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

        private void OnShutterTestStatusChanged(
            NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message?.Data is null || message.Data.BayNumber == 0)
            {
                return;
            }

            this.CompletedCycles = message.Data.ExecutedCycles;

            if (this.InputRequiredCycles <= message.Data.ExecutedCycles)
            {
                this.IsExecutingProcedure = false;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task RetrieveTestParametersAsync()
        {
            try
            {
                var procedureParameters = await this.shuttersService.GetTestParametersAsync();

                this.InputRequiredCycles = procedureParameters.RequiredCycles;
                this.InputDelayBetweenCycles = procedureParameters.DelayBetweenCycles;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void UpdateError()
        {
            this.IsExecutingProcedure = false;
        }

        #endregion
    }
}
