using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public partial class BaseMovementsViewModel : BaseMainViewModel, IRegionMemberLifetime
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private SubscriptionToken fsmExceptionToken;

        private bool isExecutingProcedure;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private bool isStopping;

        private int? loadingUnitId;
        
        private SubscriptionToken moveLoadingUnitToken;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public BaseMovementsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));
        }

        #endregion

        #region Properties

        public int? CurrentMissionId { get; private set; }

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

        public bool IsLoadingUnitIdValid
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return false;
                }

                return this.MachineService.Loadunits.Any(l => l.Id == this.loadingUnitId.Value);
            }
        }

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionDownSelected, value))
                {
                    this.IsPositionUpSelected = !this.isPositionDownSelected;
                }
            }
        }

        public bool IsPositionUpSelected
        {
            get => this.isPositionUpSelected;
            set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value) && value)
                {
                    this.IsPositionDownSelected = !this.isPositionUpSelected;
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            set => this.SetProperty(ref this.isStopping, value);
        }

        public virtual bool KeepAlive => true;

        public int? LoadingUnitCellId
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return null;
                }

                return this.MachineService.Loadunits.FirstOrDefault(l => l.Id == this.loadingUnitId.Value)?.CellId;
            }
        }

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IMachineLoadingUnitsWebService MachineLoadingUnitsWebService => this.machineLoadingUnitsWebService;

        public ICommand SelectBayPositionDownCommand =>
            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(
                this.SelectBayPositionDown,
                this.CanSelectBayPositionDown));

        public ICommand SelectBayPositionUpCommand =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(
                this.SelectBayPositionUp,
                this.CanSelectBayPositionUp));

        public ICommand StartCommand =>
               this.startCommand
               ??
               (this.startCommand = new DelegateCommand(
                   async () => await this.StartAsync(),
                   this.CanStart));

        public ICommand StopCommand =>
                this.stopCommand
                ??
                (this.stopCommand = new DelegateCommand(
                    async () => await this.StopAsync(),
                    this.CanStop));

        #endregion

        #region Methods

        public virtual bool CanSelectBayPositionDown()
        {
            return !this.IsExecutingProcedure &&
                   this.IsPositionUpSelected;
        }

        public virtual bool CanSelectBayPositionUp()
        {
            return !this.IsExecutingProcedure &&
                   !this.IsPositionUpSelected;
        }

        public virtual bool CanStart()
        {
            return
                !this.IsExecutingProcedure;
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.moveLoadingUnitToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>().Unsubscribe(this.moveLoadingUnitToken);
                this.moveLoadingUnitToken?.Dispose();
                this.moveLoadingUnitToken = null;
            }

            if (this.fsmExceptionToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<FsmExceptionMessageData>>().Unsubscribe(this.fsmExceptionToken);
                this.fsmExceptionToken?.Dispose();
                this.fsmExceptionToken = null;
            }
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSource(bool isPositionDownSelected)
        {
            return this.GetLoadingUnitSourceByDestination(this.MachineService.BayNumber, isPositionDownSelected);
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSourceByDestination(MAS.AutomationService.Contracts.BayNumber bayNumber, bool isPositionDownSelected)
        {
            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
                }
            }

            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
                }
            }

            if (bayNumber == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (isPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay3Up;
                }
            }

            return MAS.AutomationService.Contracts.LoadingUnitLocation.NoLocation;
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        public virtual void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
        }

        public virtual void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();

            this.RaiseCanExecuteChanged();
        }

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync()
        {
            try
            {
                this.IsBackNavigationAllowed = true;
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.StopAsync(null, this.MachineService.BayNumber);

                this.IsStopping = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            this.RestoreStates();
        }

        protected virtual void Ended()
        {
            this.RestoreStates();

            this.ShowNotification(
                VW.App.Resources.InstallationApp.ProcedureCompleted,
                Services.Models.NotificationSeverity.Success);
        }

        protected virtual void Error(Exception ex, string errorMessage)
        {
            this.IsBackNavigationAllowed = true;
            this.IsStopping = false;

            this.RestoreStates();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                this.ShowNotification(errorMessage, Services.Models.NotificationSeverity.Error);
            }
            else
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.RestoreStates();
            }
        }

        protected virtual void OnWaitResume()
        {
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.startCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.selectBayPositionDownCommand?.RaiseCanExecuteChanged();
            this.selectBayPositionUpCommand?.RaiseCanExecuteChanged();
        }

        private bool CanStop()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsStopping;
        }

        private void OnFsmException(NotificationMessageUI<FsmExceptionMessageData> message)
        {
            var data = message?.Data as FsmExceptionMessageData;
            var ex = data?.InnerException as Exception;

            this.Error(ex, data.ExceptionDescription);
        }

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsBackNavigationAllowed = false;
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    this.CurrentMissionId = (message.Data as MoveLoadingUnitMessageData).MissionId;

                    break;

                case MessageStatus.OperationWaitResume:
                    this.OnWaitResume();
                    break;

                case MessageStatus.OperationEnd:
                    {
                        this.IsBackNavigationAllowed = true;
                        if (!this.IsExecutingProcedure)
                        {
                            break;
                        }

                        this.Ended();

                        break;
                    }

                case MessageStatus.OperationStop:
                case MessageStatus.OperationFaultStop:
                case MessageStatus.OperationRunningStop:
                    {
                        this.IsBackNavigationAllowed = true;
                        this.Stopped();

                        break;
                    }

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;
                    this.IsBackNavigationAllowed = true;

                    break;
            }
        }

        private void RestoreStates()
        {
            this.IsExecutingProcedure = false;

            this.RaiseCanExecuteChanged();
        }

        private void Stopped()
        {
            this.IsStopping = false;
            this.RestoreStates();

            this.ShowNotification(
                Resources.InstallationApp.ProcedureWasStopped,
                Services.Models.NotificationSeverity.Warning);
        }

        private void SubscribeToEvents()
        {
            this.moveLoadingUnitToken = this.moveLoadingUnitToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        this.OnMoveLoadingUnitChanged,
                        ThreadOption.UIThread,
                        false);

            this.fsmExceptionToken = this.fsmExceptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<FsmExceptionMessageData>>()
                    .Subscribe(
                        this.OnFsmException,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
