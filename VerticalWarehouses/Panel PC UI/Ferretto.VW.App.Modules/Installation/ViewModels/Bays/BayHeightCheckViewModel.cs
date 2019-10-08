using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BayHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineBaysService machineBaysService;

        private readonly IMachineElevatorService machineElevatorService;

        private DelegateCommand applyCorrectionCommand;

        private DelegateCommand changeToLowerBayPositionCommand;

        private DelegateCommand changeToUpperBayPositionCommand;

        private int currentBayPosition;

        private double? currentHeight;

        private double? displacement;

        private double? inputStepValue;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveToBayHeightCommand;

        private DelegateCommand moveUpCommand;

        private double positionHeight;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BayHeightCheckViewModel(
            IBayManager bayManager,
            IMachineElevatorService machineElevatorService,
            IMachineBaysService machineBaysService)
            : base(PresentationMode.Installer)
        {
            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineBaysService is null)
            {
                throw new ArgumentNullException(nameof(machineBaysService));
            }

            this.Bay = bayManager.Bay;
            this.machineElevatorService = machineElevatorService;
            this.machineBaysService = machineBaysService;
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(),
                this.CanApplyCorrectionCommand));

        public Bay Bay { get; }

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
            protected set
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

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    message => this.OnAutomationMessageReceived(message),
                    ThreadOption.UIThread,
                    false);

            this.IsBackNavigationAllowed = true;

            this.ChangeDataFromBayPosition();

            this.InitializeDataAsync();
        }

        protected void RaiseCanExecuteChanged()
        {
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.changeToUpperBayPositionCommand?.RaiseCanExecuteChanged();
            this.changeToLowerBayPositionCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                await this.machineBaysService.UpdateHeightAsync(this.currentBayPosition, this.PositionHeight + this.Displacement.Value);

                this.ChangeDataFromBayPosition();

                this.ShowNotification(
                    $"Quota posizione {this.currentBayPosition} aggiornata.",
                    Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
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
                this.CurrentHeight != this.PositionHeight;
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
                ? this.Bay.Positions.Min(p => p.Height)
                : this.Bay.Positions.Max(p => p.Height);

            this.RaiseCanExecuteChanged();

            this.InputStepValue = null;
        }

        private async Task InitializeDataAsync()
        {
            this.CurrentBayPosition = 1;

            this.IsElevatorMovingDown = false;
            this.IsElevatorMovingUp = false;
            this.IsElevatorMovingToHeight = false;

            try
            {
                this.CurrentHeight = await this.machineElevatorService.GetVerticalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task MoveDownAsync()
        {
            try
            {
                this.IsElevatorMovingDown = true;

                await this.machineElevatorService.MoveVerticalOfDistanceAsync(-this.InputStepValue.Value);
                this.Displacement = (this.Displacement ?? 0) - this.InputStepValue.Value;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
        }

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                this.IsElevatorMovingToHeight = true;
                await this.machineElevatorService.MoveToVerticalPositionAsync(this.PositionHeight, FeedRateCategory.BayHeight);
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToHeight = false;
                this.ShowNotification(ex);
            }
        }

        private async Task MoveUpAsync()
        {
            try
            {
                this.IsElevatorMovingUp = true;

                await this.machineElevatorService.MoveVerticalOfDistanceAsync(this.InputStepValue.Value);
                this.Displacement = (this.Displacement ?? 0) + this.InputStepValue.Value;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
        }

        private void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Status == MessageStatus.OperationEnd
                ||
                message.Status == MessageStatus.OperationStop)
            {
                this.IsElevatorMovingUp = false;
                this.IsElevatorMovingDown = false;
                this.IsElevatorMovingToHeight = false;
            }

            if (message?.Data?.CurrentPosition != null)
            {
                this.CurrentHeight = message.Data.CurrentPosition;
            }
        }

        private void ToggleBayPosition()
        {
            this.CurrentBayPosition = this.CurrentBayPosition == 1 ? 2 : 1;
        }

        #endregion
    }
}
