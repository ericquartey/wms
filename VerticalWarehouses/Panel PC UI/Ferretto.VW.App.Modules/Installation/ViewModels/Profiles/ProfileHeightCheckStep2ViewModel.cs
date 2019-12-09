using System;
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
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ProfileHeightCheckStep2ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly ISensorsService sensorsService;

        private double? bayPositionHeight;

        private double? currentPosition;

        private SubscriptionToken elevatorToken;

        private DelegateCommand goToSemiAutomaticMovmentsCommand;

        private bool isElevatorMovingToBay;

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private DelegateCommand moveToBayHeightCommand;

        private SubscriptionToken positioningToken;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep2ViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IMachineElevatorService machineElevatorService,
            IBayManager bayManager,
            ISensorsService sensorsService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
        }

        #endregion

        #region Properties

        public double? BayPositionHeight
        {
            get => this.bayPositionHeight;
            private set => this.SetProperty(ref this.bayPositionHeight, value);
        }

        public double? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        public ICommand GoToSemiAutomaticMovmentsCommand =>
            this.goToSemiAutomaticMovmentsCommand
            ??
            (this.goToSemiAutomaticMovmentsCommand = new DelegateCommand(
                async () => await this.GoToSemiAutomaticMovmentsAsync(), () => true));

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsElevatorMovingToBay));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPosition1Selected
        {
            get => this.isPosition1Selected;
            private set
            {
                if (this.SetProperty(ref this.isPosition1Selected, value))
                {
                    this.IsPosition2Selected = !this.IsPosition1Selected;
                }
            }
        }

        public bool IsPosition2Selected
        {
            get => this.isPosition2Selected;
            private set
            {
                if (this.SetProperty(ref this.isPosition2Selected, value))
                {
                    this.IsPosition1Selected = !this.IsPosition2Selected;
                }
            }
        }

        public ICommand MoveToBayHeightCommand =>
            this.moveToBayHeightCommand
            ??
            (this.moveToBayHeightCommand = new DelegateCommand(
                async () => await this.MoveToBayHeightAsync(),
                this.CanMoveToBayHeight));

        public ICommand SelectBayPosition1Command =>
            this.selectBayPosition1Command
            ??
            (this.selectBayPosition1Command = new DelegateCommand(() => this.SelectBayPosition1(), this.CanSelectBayPosition));

        public ICommand SelectBayPosition2Command =>
            this.selectBayPosition2Command
            ??
            (this.selectBayPosition2Command = new DelegateCommand(() => this.SelectBayPosition2(), this.CanSelectBayPosition));

        public ISensorsService Sensors => this.sensorsService;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.ElevatorPosition;

            this.elevatorToken = this.elevatorToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.positioningToken = this.EventAggregator
              .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
              .Subscribe(
                  message =>
                  {
                      this.CurrentPosition = this.machineElevatorService.Position.Vertical;
                  },
                  ThreadOption.UIThread,
                  false);

            try
            {
                this.CurrentPosition = this.machineElevatorService.Position.Vertical;

                this.SelectBayPosition1();

                await this.sensorsService.RefreshAsync(true);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.moveToBayHeightCommand?.RaiseCanExecuteChanged();
            this.selectBayPosition1Command?.RaiseCanExecuteChanged();
            this.selectBayPosition2Command?.RaiseCanExecuteChanged();
            this.goToSemiAutomaticMovmentsCommand?.RaiseCanExecuteChanged();
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP1);
            this.ShowNextStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP3);
            this.ShowAbortStep(true, true);
        }

        private bool CanMoveToBayHeight()
        {
            return this.BayPositionHeight.HasValue
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMovingToBay
                &&
                (this.sensorsService.IsZeroChain || (this.sensorsService.Sensors.LuPresentInMachineSide && this.sensorsService.Sensors.LuPresentInOperatorSide));
        }

        private bool CanSelectBayPosition()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMovingToBay
                &&
                (this.sensorsService.IsZeroChain || (this.sensorsService.Sensors.LuPresentInMachineSide && this.sensorsService.Sensors.LuPresentInOperatorSide));
        }

        private async Task GoToSemiAutomaticMovmentsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.SEMIAUTOMOVEMENTS,
                    null,
                    trackCurrentView: true);
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

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.BayPositionHeight.Value,
                    false, false);

                this.IsElevatorMovingToBay = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToBay = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting:
                    {
                        this.IsElevatorMovingToBay = true;
                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsElevatorMovingToBay = false;
                        break;
                    }
            }
        }

        private void SelectBayPosition1()
        {
            this.IsPosition1Selected = true;
            this.BayPositionHeight = this.Bay.Positions.First().Height;
            this.RaiseCanExecuteChanged();
        }

        private void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
            this.BayPositionHeight = this.Bay.Positions.Last().Height;
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
