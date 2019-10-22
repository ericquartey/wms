﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BayHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand applyCorrectionCommand;

        private Bay bay;

        private DelegateCommand changeToLowerBayPositionCommand;

        private DelegateCommand changeToUpperBayPositionCommand;

        private int currentBayPosition;

        private double? currentHeight;

        private double? displacement;

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
            IMachineBaysWebService machineBaysWebService)
            : base(PresentationMode.Installer)
        {
            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.bayManager = bayManager;
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
        }

        #endregion

        #region Properties

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
                this.EventAggregator.SubscribeToEvent<PositioningMessageData>(
                    this.OnAutomationMessageReceived);

            this.IsBackNavigationAllowed = true;

            this.Bay = await this.bayManager.GetBayAsync();

            await this.InitializeDataAsync();

            this.ChangeDataFromBayPosition();

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
                ? this.Bay.Positions.Min(p => p.Height)
                : this.Bay.Positions.Max(p => p.Height);

            this.RaiseCanExecuteChanged();
        }

        private async Task InitializeDataAsync()
        {
            this.CurrentBayPosition = 1;

            this.IsElevatorMovingDown = false;
            this.IsElevatorMovingUp = false;
            this.IsElevatorMovingToHeight = false;

            try
            {
                this.IsWaitingForResponse = true;
                this.CurrentHeight = await this.machineElevatorWebService.GetVerticalPositionAsync();
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
                    this.procedureParameters.FeedRate);

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

            this.CurrentHeight = message.Data?.CurrentPosition ?? this.CurrentHeight;
        }

        private void ToggleBayPosition()
        {
            this.CurrentBayPosition = this.CurrentBayPosition == 1 ? 2 : 1;
        }

        #endregion
    }
}
