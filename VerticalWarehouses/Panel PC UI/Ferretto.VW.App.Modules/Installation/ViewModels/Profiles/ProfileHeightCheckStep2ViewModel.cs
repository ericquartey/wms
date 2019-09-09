using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ProfileHeightCheckStep2ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private bool canExecuteMoveDownCommand;

        private bool canExecuteMoveUpCommand;

        private decimal? currentPosition;

        private bool isMovingDown;

        private bool isMovingUp;

        private bool isStopping;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private DelegateCommand stopMovementCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public ProfileHeightCheckStep2ViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager,
            IMachineElevatorService machineElevatorService)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
            if (machineElevatorService is null)
            {
                throw new System.ArgumentNullException(nameof(machineElevatorService));
            }

            this.machineElevatorService = machineElevatorService;

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public bool CanExecuteMoveDownCommand
        {
            get => this.canExecuteMoveDownCommand;
            private set => this.SetProperty(ref this.canExecuteMoveDownCommand, value);
        }

        public bool CanExecuteMoveUpCommand
        {
            get => this.canExecuteMoveUpCommand;
            private set => this.SetProperty(ref this.canExecuteMoveUpCommand, value);
        }

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        public bool IsMovingDown
        {
            get => this.isMovingDown;
            private set
            {
                if (this.SetProperty(ref this.isMovingDown, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsMovingUp
        {
            get => this.isMovingUp;
            private set
            {
                if (this.SetProperty(ref this.isMovingUp, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            private set
            {
                if (this.SetProperty(ref this.isStopping, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(async () => await this.MoveDownAsync()));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(async () => await this.MoveUpAsync()));

        public DelegateCommand StopMovementCommand =>
            this.stopMovementCommand
            ??
            (this.stopMovementCommand = new DelegateCommand(async () => await this.StopMovementAsync()));

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

        public async Task MoveDownAsync()
        {
            this.IsMovingDown = true;
            this.IsMovingUp = false;

            await this.StartMovementAsync(VerticalMovementDirection.Down);
        }

        public async Task MoveUpAsync()
        {
            this.IsMovingUp = true;
            this.IsMovingDown = false;

            await this.StartMovementAsync(VerticalMovementDirection.Up);
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  message =>
                  {
                      var c = message?.Data?.CurrentPosition;
                      if (c.HasValue && c != 0)
                      {
                          this.CurrentPosition = c;
                      }
                  },
                  ThreadOption.UIThread,
                  false);

            await this.RetrieveCurrentPositionAsync();
        }

        protected async Task StopMovementAsync()
        {
            try
            {
                this.IsStopping = true;

                await this.machineElevatorService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsMovingDown = false;
                this.IsMovingUp = false;
                this.IsStopping = false;
            }
        }

        private void RefreshCanExecuteCommands()
        {
            this.CanExecuteMoveUpCommand = !this.IsMovingDown && !this.IsStopping;
            this.CanExecuteMoveDownCommand = !this.IsMovingUp && !this.IsStopping;
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = await this.machineElevatorService.GetVerticalPositionAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task StartMovementAsync(VerticalMovementDirection direction)
        {
            try
            {
                await this.machineElevatorService.MoveVerticalAsync(direction);
            }
            catch (System.Exception ex)
            {
                this.IsMovingUp = false;
                this.IsMovingDown = false;

                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
