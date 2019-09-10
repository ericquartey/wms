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

        private decimal? currentPosition;

        private DelegateCommand goToBayCommand;

        private decimal positionBay;

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

            this.ChangeDataFromBayPosition();

            this.RefreshCanExecuteCommands();
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

        public ICommand GoToBayCommand =>
            this.goToBayCommand
            ??
            (this.goToBayCommand = new DelegateCommand(
                async () => await this.GoToBayCommandAsync(),
                this.CanExecuteGoToBayCommand));

        public decimal PositionBay
        {
            get => this.positionBay;
            set
            {
                if (this.SetProperty(ref this.positionBay, value))
                {
                    this.RefreshCanExecuteCommands();
                }
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

        public async Task GoToBayCommandAsync()
        {
            try
            {
                await this.machineElevatorService.MoveToVerticalPositionAsync(this.PositionBay, FeedRateCategory.BayHeight);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
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

                      this.RefreshCanExecuteCommands();
                  },
                  ThreadOption.UIThread,
                  false);

            await this.RetrieveCurrentPositionAsync();
        }

        protected bool CanExecuteGoToBayCommand()
        {
            return //!this.IsExecutingProcedure &&
                    this.CurrentPosition != this.PositionBay;
        }

        protected override bool CanExecuteStep3Command()
        {
            return base.CanExecuteStepCommand(); //&& this.CurrentPosition == this.PositionBay;
        }

        private void ChangeDataFromBayPosition()
        {
            if (this.BayManager.Bay.Number == 1)
            {
                this.PositionBay = this.BayManager.Bay.Positions.First();
            }
            else
            {
                this.PositionBay = this.BayManager.Bay.Positions.Last();
            }
        }

        private void RefreshCanExecuteCommands()
        {
            base.RaiseCanExecuteChanged();
            this.goToBayCommand?.RaiseCanExecuteChanged();
        }

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = await this.machineElevatorService.GetVerticalPositionAsync();
                this.RefreshCanExecuteCommands();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
