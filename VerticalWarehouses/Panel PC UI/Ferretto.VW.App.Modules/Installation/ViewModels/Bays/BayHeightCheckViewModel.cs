using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class BayHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand abortCommand;

        private DelegateCommand applyCorrectionCommand;

        private Bay bay;

        private DelegateCommand changeToLowerBayPositionCommand;

        private DelegateCommand changeToUpperBayPositionCommand;

        private int currentBayPosition;

        private double? currentHeight;

        private double? displacement;

        private SubscriptionToken elevatorPositionChangedToken;

        private double? inputStepValue;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingUp;

        private bool isWaitingForResponse;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveToBayHeightCommand;

        private DelegateCommand moveUpCommand;

        private double positionHeight;

        private PositioningProcedure procedureParameters;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BayHeightCheckViewModel(
            IBayManager bayManager,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineElevatorService machineElevatorService)
            : base(PresentationMode.Installer)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
        }

        #endregion

        #region Properties

        public ICommand AbortCommand =>
           this.abortCommand
           ??
           (this.abortCommand = new DelegateCommand(
               async () => await this.AbortAsync(),
               this.CanAbort));

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(),
                this.CanApplyCorrectionCommand));

        public Bay Bay
        {
            get => this.bay;
            private set => this.SetProperty(ref this.bay, value);
        }

        public ICommand ChangeToLowerBayPositionCommand =>
            this.changeToLowerBayPositionCommand
            ??
            (this.changeToLowerBayPositionCommand = new DelegateCommand(
                this.ToggleBayPosition,
                this.CanChangeCurrentPosition2));

        public ICommand ChangeToUpperBayPositionCommand =>
            this.changeToUpperBayPositionCommand
            ??
            (this.changeToUpperBayPositionCommand = new DelegateCommand(
                this.ToggleBayPosition,
                this.CanChangeCurrentPosition1));

        public int CurrentBayPosition
        {
            get => this.currentBayPosition;
            set
            {
                if (this.SetProperty(ref this.currentBayPosition, value))
                {
                    this.ChangeDataFromBayPosition();
                }
            }
        }

        public double? CurrentHeight
        {
            get => this.currentHeight;
            set
            {
                if (this.SetProperty(ref this.currentHeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double? Displacement
        {
            get => this.displacement;
            set
            {
                if (this.SetProperty(ref this.displacement, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputStepValue)]);

        public double? InputStepValue
        {
            get => this.inputStepValue;
            set
            {
                if (this.SetProperty(ref this.inputStepValue, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingDown
        {
            get => this.isElevatorMovingDown;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingDown, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToHeight
        {
            get => this.isElevatorMovingToHeight;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToHeight, value))
                {
                    if (this.isElevatorMovingToHeight)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingUp
        {
            get => this.isElevatorMovingUp;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingUp, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
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

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(
                async () => await this.MoveDownAsync(),
                this.CanExecuteMoveDownCommand));

        public ICommand MoveToBayHeightCommand =>
            this.moveToBayHeightCommand
            ??
            (this.moveToBayHeightCommand = new DelegateCommand(
                async () => await this.MoveToBayHeightAsync(),
                this.CanExecuteMoveToBayHeight));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(
                async () => await this.MoveUpAsync(),
                this.CanExecuteMoveUpCommand));

        public double PositionHeight
        {
            get => this.positionHeight;
            set
            {
                if (this.SetProperty(ref this.positionHeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputStepValue):
                        if (!this.InputStepValue.HasValue)
                        {
                            return $"Step is required.";
                        }

                        if (this.InputStepValue.Value < 0)
                        {
                            return "Step must be positive.";
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
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnAutomationMessageReceived,
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.IsBackNavigationAllowed = true;

            await this.InitializeDataAsync();

            try
            {
                this.procedureParameters = await this.machineBaysWebService.GetHeightCheckParametersAsync();

                this.InputStepValue = this.procedureParameters.Step;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task AbortAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.StopAsync();

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsElevatorMovingToHeight = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsElevatorMovingToHeight = false;
            }
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.Bay = await this.machineBaysWebService.UpdateHeightAsync(
                    this.currentBayPosition,
                    this.PositionHeight + this.Displacement.Value);

                this.Displacement = null;

                this.ChangeDataFromBayPosition();

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
                    Services.Models.NotificationSeverity.Success);
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

        private bool CanAbort()
        {
            return
                this.IsElevatorMovingToHeight
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanApplyCorrectionCommand()
        {
            return
                !this.IsElevatorMovingToHeight
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                this.currentHeight.HasValue
                &&
                this.CurrentHeight != this.PositionHeight
                &&
                this.Displacement.HasValue
                &&
                this.displacement.Value != 0;
        }

        private bool CanChangeCurrentPosition1()
        {
            return !this.isElevatorMovingDown
                &&
                !this.isElevatorMovingToHeight
                &&
                !this.isElevatorMovingUp
                &&
                this.CurrentBayPosition == 2;
        }

        private bool CanChangeCurrentPosition2()
        {
            return !this.isElevatorMovingDown
                &&
                !this.isElevatorMovingToHeight
                &&
                !this.isElevatorMovingUp
                &&
                this.CurrentBayPosition == 1;
        }

        private bool CanExecuteMoveDownCommand()
        {
            return
                !this.IsElevatorMovingToHeight
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                string.IsNullOrWhiteSpace(this[nameof(this.InputStepValue)]);
        }

        private bool CanExecuteMoveToBayHeight()
        {
            return !this.IsElevatorMovingToHeight
                   &&
                   !this.IsElevatorMovingUp
                   &&
                   !this.IsElevatorMovingDown;
        }

        private bool CanExecuteMoveUpCommand()
        {
            return
                !this.IsElevatorMovingToHeight
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                string.IsNullOrWhiteSpace(this[nameof(this.InputStepValue)]);
        }

        private void ChangeDataFromBayPosition()
        {
            this.PositionHeight = this.CurrentBayPosition == 1
                ? this.Bay.Positions.Max(p => p.Height)
                : this.Bay.Positions.Min(p => p.Height);

            this.RaiseCanExecuteChanged();
        }

        private async Task InitializeDataAsync()
        {
            this.IsElevatorMovingDown = false;
            this.IsElevatorMovingUp = false;
            this.IsElevatorMovingToHeight = false;

            try
            {
                this.IsWaitingForResponse = true;

                this.Bay = await this.bayManager.GetBayAsync();

                this.CurrentHeight = this.machineElevatorService.Position.Vertical;

                this.CurrentBayPosition = 2;
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

        private async Task MoveDownAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsElevatorMovingDown = true;

                await this.machineElevatorWebService.MoveVerticalOfDistanceAsync(-this.InputStepValue.Value);
                this.Displacement = (this.Displacement ?? 0) - this.InputStepValue.Value;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                this.IsElevatorMovingToHeight = true;
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.PositionHeight,
                    this.procedureParameters.FeedRate,
                    false,
                    true);

                this.Displacement = null;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToHeight = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveUpAsync()
        {
            try
            {
                this.IsElevatorMovingUp = true;
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveVerticalOfDistanceAsync(this.InputStepValue.Value);
                this.Displacement = (this.Displacement ?? 0) + this.InputStepValue.Value;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.IsNotRunning())
            {
                this.IsElevatorMovingUp = false;
                this.IsElevatorMovingDown = false;
                this.IsElevatorMovingToHeight = false;

                if (message.IsErrored())
                {
                    this.ShowNotification(message.Description);
                }
            }
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.CurrentHeight = e.VerticalPosition;
        }

        private void RaiseCanExecuteChanged()
        {
            this.abortCommand?.RaiseCanExecuteChanged();
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.changeToUpperBayPositionCommand?.RaiseCanExecuteChanged();
            this.changeToLowerBayPositionCommand?.RaiseCanExecuteChanged();
        }

        private void ToggleBayPosition()
        {
            this.CurrentBayPosition = this.CurrentBayPosition == 1 ? 2 : 1;
        }

        #endregion
    }
}
