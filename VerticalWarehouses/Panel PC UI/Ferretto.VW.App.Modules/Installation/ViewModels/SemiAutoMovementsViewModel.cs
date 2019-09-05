using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class SemiAutoMovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineCellsService machineCellsService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly IMachineLoadingUnitsService machineLoadingUnitsService;

        private readonly DelegateCommand stopMovementCommand;

        private bool canInputCellId;

        private IEnumerable<Cell> cells;

        private decimal? elevatorHorizontalPosition;

        private decimal? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private int? inputCellId;

        private int? inputLoadingUnitCode;

        private bool isElevatorMoving;

        private bool isWaitingForResponse;

        private LoadingUnit loadingUnitInBay;

        private IEnumerable<LoadingUnit> loadingUnits;

        private Cell selectedCell;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public SemiAutoMovementsViewModel(
            IMachineElevatorService machineElevatorService,
            IMachineCellsService machineCellsService,
            IMachineLoadingUnitsService machineLoadingUnitsService,
            IBayManager bayManagerService)
            : base(PresentationMode.Installer)
        {
            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (machineCellsService is null)
            {
                throw new ArgumentNullException(nameof(machineCellsService));
            }

            if (machineLoadingUnitsService is null)
            {
                throw new ArgumentNullException(nameof(machineLoadingUnitsService));
            }

            if (bayManagerService is null)
            {
                throw new ArgumentNullException(nameof(bayManagerService));
            }

            this.machineElevatorService = machineElevatorService;
            this.machineCellsService = machineCellsService;
            this.machineLoadingUnitsService = machineLoadingUnitsService;
            this.bayManagerService = bayManagerService;
        }

        #endregion

        #region Properties

        public int BayNumber => this.bayManagerService.Bay.Number;

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public decimal? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            protected set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public decimal? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            protected set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

        public LoadingUnit EmbarkedLoadingUnit
        {
            get => this.embarkedLoadingUnit;
            protected set => this.SetProperty(ref this.embarkedLoadingUnit, value);
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set
            {
                if (this.SetProperty(ref this.inputCellId, value)
                    &&
                    this.Cells != null)
                {
                    this.SelectedCell = value == null
                        ? null
                        : this.Cells.SingleOrDefault(c => c.Id == value);
                }
            }
        }

        public int? InputLoadingUnitCode
        {
            get => this.inputLoadingUnitCode;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitCode, value)
                    &&
                    this.LoadingUnits != null)
                {
                    this.LoadingUnitInBay = value == null
                        ? null
                        : this.LoadingUnits.SingleOrDefault(l => l.Id == value);
                }
            }
        }

        public bool IsElevatorMoving
        {
            get => this.isElevatorMoving;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMoving, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ShowNotification(string.Empty, Services.Models.NotificationSeverity.Clear);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public LoadingUnit LoadingUnitInBay
        {
            get => this.loadingUnitInBay;
            protected set => this.SetProperty(ref this.loadingUnitInBay, value);
        }

        public IEnumerable<LoadingUnit> LoadingUnits { get => this.loadingUnits; set => this.loadingUnits = value; }

        public Cell SelectedCell
        {
            get => this.selectedCell;
            protected set
            {
                if (this.SetProperty(ref this.selectedCell, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        protected IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
                    this.RaiseCanExecuteChanged();
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

        public override async Task OnNavigatedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
              .GetEvent<NotificationEventUI<PositioningMessageData>>()
              .Subscribe(
                  message => this.OnElevatorPositionChanged(message),
                  ThreadOption.UIThread,
                  false);

            await this.RetrieveElevatorPositionAsync();

            await this.RetrieveCellsAsync();

            await this.RetrieveLoadingUnitsAsync();

            await base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.RetrieveElevatorPositionAsync();
        }

        public async Task RetrieveLoadingUnitsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.loadingUnits = await this.machineLoadingUnitsService.GetAllAsync();
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

        private void OnElevatorPositionChanged(CommonUtils.Messages.NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
            }

            if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationExecuting)
            {
                if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Vertical)
                {
                    this.ElevatorVerticalPosition = message.Data.CurrentPosition;
                }
                else if (message.Data.AxisMovement == CommonUtils.Messages.Enumerations.Axis.Horizontal)
                {
                    this.ElevatorHorizontalPosition = message.Data.CurrentPosition;
                }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.CanInputCellId = this.Cells != null
               &&
               !this.IsElevatorMoving
               &&
               !this.IsWaitingForResponse;
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.Cells = await this.machineCellsService.GetAllAsync();
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

        private async Task RetrieveElevatorPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.ElevatorVerticalPosition = await this.machineElevatorService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorService.GetHorizontalPositionAsync();
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

        #endregion
    }
}
