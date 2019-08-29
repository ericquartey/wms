using System;
using System.Collections.Generic;
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

        private readonly IBayManager bayManager;

        private readonly List<PositionInfo> bayPositions = new List<PositionInfo>();

        private readonly IMachineElevatorService machineElevatorService;

        private DelegateCommand changeBayPosition1Command;

        private DelegateCommand changeBayPosition2Command;

        private int currentBayPosition;

        private decimal currentHeight;

        private decimal? inputStepValue;

        private bool isBayPositionsVisible;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveToBayHeightCommand;

        private DelegateCommand moveUpCommand;

        private decimal positionHeight;

        private DelegateCommand saveHeightCorrectionCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BayHeightCheckViewModel(
            IBayManager bayManager,
            IMachineElevatorService machineElevatorService) : base(PresentationMode.Installer)
        {
            this.bayManager = bayManager;
            this.machineElevatorService = machineElevatorService;
        }

        #endregion

        #region Properties

        public IEnumerable<PositionInfo> BayPositions => new List<PositionInfo>(this.bayPositions);

        public ICommand ChangeBayPosition1Command =>
            this.changeBayPosition1Command
            ??
            (this.changeBayPosition1Command = new DelegateCommand(
                this.ChangeCurrentPosition1,
                this.CanChangeCurrentPosition1));

        public ICommand ChangeBayPosition2Command =>
     this.changeBayPosition2Command
     ??
     (this.changeBayPosition2Command = new DelegateCommand(
         this.ChangeCurrentPosition2,
         this.CanChangeCurrentPosition2));

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

        public decimal CurrentHeight
        {
            get => this.currentHeight;
            set
            {
                if (this.SetProperty(ref this.currentHeight, value))
                {
                    this.UpdateChanged();
                }
            }
        }

        public string Error => string.Join(
              Environment.NewLine,
              this[nameof(this.positionHeight)]);

        public decimal? InputStepValue
        {
            get => this.inputStepValue;
            set
            {
                if (this.SetProperty(ref this.inputStepValue, value))
                {
                    this.UpdateChanged();
                }
            }
        }

        public bool IsBayPositionsVisible
        {
            get => this.isBayPositionsVisible;
            private set
            {
                if (this.SetProperty(ref this.isBayPositionsVisible, value))
                {
                    this.UpdateChanged();
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
                    this.UpdateChanged();
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
                        this.ShowNotification(string.Empty, Services.Models.NotificationSeverity.Clear);
                    }

                    this.UpdateChanged();
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
                    this.UpdateChanged();
                }
            }
        }

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(
                async () => await this.ExecuteMoveDownAsync(),
                this.CanExecuteMoveDownCommand));

        public ICommand MoveToBayHeightCommand =>
          this.moveToBayHeightCommand
          ??
          (this.moveToBayHeightCommand = new DelegateCommand(
              async () => await this.ExecuteMoveToBayHeightCommandAsync(),
              this.CanExecuteMoveToBayHeight));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(
                async () => await this.ExecuteMoveUpAsync(),
                this.CanExecuteMoveUpCommand));

        public decimal PositionHeight
        {
            get => this.positionHeight;
            set
            {
                if (this.SetProperty(ref this.positionHeight, value))
                {
                    this.UpdateChanged();
                }
            }
        }

        public ICommand SaveHeightCorrectionCommand =>
            this.saveHeightCorrectionCommand
            ??
            (this.saveHeightCorrectionCommand = new DelegateCommand(
                async () => await this.ExecuteSaveHeightCorrectionAsync(),
                this.CanExecuteApplyCorrectionCommand));

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

        public async override Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            this.EventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Unsubscribe(this.subscriptionToken);
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
            this.InitializeBayPositions();
            this.InitializeData();
            this.ChangeDataFromBayPosition();
        }

        protected void UpdateChanged()
        {
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.saveHeightCorrectionCommand?.RaiseCanExecuteChanged();
            this.changeBayPosition1Command?.RaiseCanExecuteChanged();
            this.changeBayPosition2Command?.RaiseCanExecuteChanged();
        }

        private bool CanChangeCurrentPosition(int position)
        {
            if (position == 1 &&
                !this.IsBayPositionsVisible)
            {
                return false;
            }

            return !this.isElevatorMovingDown
                    &&
                    !this.isElevatorMovingToHeight
                    &&
                    !this.isElevatorMovingUp;
        }

        private bool CanChangeCurrentPosition1()
        {
            return !this.isElevatorMovingDown
                        &&
                        !this.isElevatorMovingToHeight
                        &&
                        !this.isElevatorMovingUp
                        &&
                        this.IsBayPositionsVisible;
        }

        private bool CanChangeCurrentPosition2()
        {
            return !this.isElevatorMovingDown
             &&
             !this.isElevatorMovingToHeight
             &&
             !this.isElevatorMovingUp
             &&
             this.IsBayPositionsVisible;
        }

        private bool CanExecuteApplyCorrectionCommand()
        {
            return
                !this.IsElevatorMovingToHeight
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                this.currentHeight > 0
                &&
                this.CurrentHeight != this.PositionHeight;
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

        private void ChangeCurrentPosition(int newPosition)
        {
            this.CurrentBayPosition = newPosition;
        }

        private void ChangeCurrentPosition1()
        {
            this.CurrentBayPosition = 1;
        }

        private void ChangeCurrentPosition2()
        {
            this.CurrentBayPosition = 2;
        }

        private void ChangeDataFromBayPosition()
        {
            if (this.currentBayPosition == 1)
            {
                this.PositionHeight = this.bayManager.Bay.Positions.First();
            }
            else
            {
                this.PositionHeight = this.bayManager.Bay.Positions.Last();
            }
            this.InputStepValue = null;
        }

        private async Task ExecuteMoveDownAsync()
        {
            try
            {
                this.IsElevatorMovingDown = true;

                await this.machineElevatorService.MoveVerticalOfDistanceAsync(-this.InputStepValue.Value);
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
        }

        private async Task ExecuteMoveToBayHeightCommandAsync()
        {
            try
            {
                this.IsElevatorMovingToHeight = true;
                await this.machineElevatorService.MoveToVerticalPositionAsync(this.PositionHeight);
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToHeight = false;
                this.ShowNotification(ex);
            }
        }

        private async Task ExecuteMoveUpAsync()
        {
            try
            {
                this.IsElevatorMovingUp = true;

                await this.machineElevatorService.MoveVerticalOfDistanceAsync(this.InputStepValue.Value);
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
        }

        private async Task ExecuteSaveHeightCorrectionAsync()
        {
            try
            {
                await this.bayManager.UpdateHeightAsync(this.bayManager.Bay.Number, this.currentBayPosition, this.currentHeight);

                this.ChangeDataFromBayPosition();

                this.ShowNotification($"Quota posizione {this.currentBayPosition} aggiornata.", Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void InitializeBayPositions()
        {
            this.bayPositions.Clear();
            this.bayPositions.Add(new PositionInfo(1));
            if (this.bayManager.Bay.Positions.Count() > 1)
            {
                this.IsBayPositionsVisible = true;
                this.bayPositions.Add(new PositionInfo(2));
            }
            this.RaisePropertyChanged(nameof(this.BayPositions));
        }

        private void InitializeData()
        {
            this.currentBayPosition = 1;

            this.IsElevatorMovingDown = false;
            this.IsElevatorMovingUp = false;
            this.IsElevatorMovingToHeight = false;

            this.RaisePropertyChanged(nameof(this.IsBayPositionsVisible));
            this.RaisePropertyChanged(nameof(this.CurrentBayPosition));
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

            if (!(message.Data is null))
            {
                this.CurrentHeight = message.Data.CurrentPosition;
            }
        }

        #endregion
    }
}
