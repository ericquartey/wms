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
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public partial class BaseMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private bool bayIsMultiPosition;

        private bool isExecutingProcedure;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private bool isStopping;

        private bool isWaitingForResponse;

        private int? loadingUnitId;

        private IEnumerable<LoadingUnit> loadingUnits;

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

        public Bay Bay { get; private set; }

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public IBayManager BayManagerService => this.bayManagerService;

        public Guid? CurrentMissionId { get; private set; }

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

                return this.loadingUnits.Any(l => l.Id == this.loadingUnitId.Value);
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
                    this.IsPositionDownSelected = !this.isPositionDownSelected;
                }
            }
        }

        public bool IsStopping
        {
            get => this.isStopping;
            set => this.SetProperty(ref this.isStopping, value);
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

        public int? LoadingUnitCellId
        {
            get
            {
                if (!this.loadingUnitId.HasValue)
                {
                    return null;
                }

                return this.loadingUnits.FirstOrDefault(l => l.Id == this.loadingUnitId.Value)?.CellId;
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
                        (this.selectBayPositionDownCommand = new DelegateCommand(this.SelectBayPositionDown));

        public ICommand SelectBayPositionUpCommand =>
                                this.selectBayPositionUpCommand
                        ??
                        (this.selectBayPositionUpCommand = new DelegateCommand(this.SelectBayPositionUp));

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

        public virtual bool CanStart()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;

            this.sensorsToken?.Dispose();
            this.sensorsToken = null;
            */
        }

        public MAS.AutomationService.Contracts.LoadingUnitLocation GetLoadingUnitSource()
        {
            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne)
            {
                if (this.IsPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay1Up;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayTwo)
            {
                if (this.IsPositionDownSelected)
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Down;
                }
                else
                {
                    return MAS.AutomationService.Contracts.LoadingUnitLocation.InternalBay2Up;
                }
            }

            if (this.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayThree)
            {
                if (this.IsPositionDownSelected)
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
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.SubscribeToEvents();

            await this.RetrieveLoadingUnitsAsync();

            await this.UpdateBayAsync();

            this.RaiseCanExecuteChanged();
        }

        public virtual void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
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

        public virtual void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
        }

        public virtual void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
        }

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.StopAsync(null, this.Bay.Number);

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

        private bool CanStop()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsStopping;
        }

        private void OnMoveLoadingUnitChanged(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationStart:
                    this.IsExecutingProcedure = true;
                    this.RaiseCanExecuteChanged();

                    this.CurrentMissionId = (message.Data as MoveLoadingUnitMessageData).MissionId;

                    break;

                case MessageStatus.OperationWaitResume:
                    this.OnWaitResume();
                    break;

                case MessageStatus.OperationEnd:
                    {
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
                        this.Stopped();

                        break;
                    }

                case MessageStatus.OperationError:
                    this.IsExecutingProcedure = false;

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
        }

        private async Task UpdateBayAsync()
        {
            try
            {
                this.Bay = await this.bayManagerService.GetBayAsync();

                this.BayIsMultiPosition = this.Bay.IsDouble;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
