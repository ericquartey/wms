using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum CellsHeightCheckStep
    {
        Inizialize,

        Measured,

        Confirm,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class CellsHeightCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private bool canInputCellId;

        private IEnumerable<Cell> cells;

        private double? currentPosition;

        private CellsHeightCheckStep currentStep;

        private SubscriptionToken elevatorPositionChangedToken;

        private int? inputCellId;

        private bool isElevatorMoving;

        private bool isElevatorOperationCompleted;

        private DelegateCommand moveToCellHeightCommand;

        private SubscriptionToken positioningOperationChangedToken;

        private Cell selectedCell;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public CellsHeightCheckViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineElevatorService machineElevatorService)
            : base(PresentationMode.Installer)
        {
            this.MachineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.MachineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public double? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

        public CellsHeightCheckStep CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public bool HasStepConfirm => this.currentStep is CellsHeightCheckStep.Confirm;

        public bool HasStepInitialize => this.currentStep is CellsHeightCheckStep.Inizialize;

        public bool HasStepMeasured => this.currentStep is CellsHeightCheckStep.Measured;

        public int? InputCellId
        {
            get => this.inputCellId;
            set
            {
                if (this.SetProperty(ref this.inputCellId, value)
                    &&
                    this.Cells != null)
                {
                    this.UpdateSelectedCell();
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

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
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

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public ICommand MoveToCellHeightCommand =>
            this.moveToCellHeightCommand
            ??
            (this.moveToCellHeightCommand = new DelegateCommand(
                async () => await this.MoveToCellHeightAsync(),
                this.CanMoveToCellHeight));

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

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

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

        protected IMachineCellsWebService MachineCellsWebService { get; }

        protected IMachineElevatorWebService MachineElevatorWebService { get; }

        protected PositioningProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter();

            this.CurrentPosition = this.machineElevatorService.Position.Vertical;

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                this.UpdateSelectedCell();

                this.Cells = await this.MachineCellsWebService.GetAllAsync();

                this.ProcedureParameters = await this.MachineCellsWebService.GetHeightCheckProcedureParametersAsync();
            }
            catch (HttpRequestException ex)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.CurrentPosition = e.VerticalPosition;
        }

        protected void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.IsErrored())
            {
                this.IsElevatorMoving = false;
                this.ShowSteps();
            }
            else if (message.IsNotRunning())
            {
                this.IsElevatorMoving = false;
                this.isElevatorOperationCompleted = true;
                this.ShowSteps();

                switch (message.Status)
                {
                    case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                        {
                            this.NavigateToNextStep();

                            break;
                        }

                    case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                        {
                            this.IsElevatorMoving = false;

                            this.ShowNotification(
                                VW.App.Resources.InstallationApp.ProcedureWasStopped,
                                Services.Models.NotificationSeverity.Warning);

                            break;
                        }
                }
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case CellsHeightCheckStep.Inizialize:
                    if (e.Next)
                    {
                        this.CurrentStep = CellsHeightCheckStep.Measured;
                    }

                    break;

                case CellsHeightCheckStep.Measured:
                    if (e.Next)
                    {
                        this.CurrentStep = CellsHeightCheckStep.Confirm;
                    }
                    else
                    {
                        this.CurrentStep = CellsHeightCheckStep.Inizialize;
                    }

                    break;

                case CellsHeightCheckStep.Confirm:
                    if (!e.Next)
                    {
                        this.CurrentStep = CellsHeightCheckStep.Measured;
                    }

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.CanInputCellId =
                this.Cells != null
                &&
                !this.IsElevatorMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanMoveToCellHeight()
        {
            return
                this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanStop()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.IsElevatorMoving;
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.CellsHeightCheck.STEP1,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step1,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.CellsHeightCheck.STEP2,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step2,
                    trackCurrentView: false));
        }

        private async Task MoveToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.MachineElevatorWebService.MoveToCellAsync(
                    this.SelectedCell.Id,
                    computeElongation: true,
                    performWeighting: false);

                this.IsElevatorMoving = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMoving = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void NavigateToNextStep()
        {
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.CellsHeightCheck.STEP1))
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.CellsHeightCheck.STEP2,
                    this.SelectedCell,
                    trackCurrentView: false);
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, false);
            this.ShowNextStep(true, this.isElevatorOperationCompleted, nameof(Utils.Modules.Installation), Utils.Modules.Installation.CellsHeightCheck.STEP2);
            this.ShowAbortStep(true, true);
        }

        private async Task StopAsync()
        {
            try
            {
                await this.MachineElevatorWebService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningOperationChanged,
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
        }

        private void UpdateSelectedCell()
        {
            this.SelectedCell = this.inputCellId == null
                        ? null
                        : this.Cells.SingleOrDefault(c => c.Id == this.inputCellId);
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case CellsHeightCheckStep.Inizialize:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CellsHeightCheckStep.Measured:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CellsHeightCheckStep.Confirm:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepInitialize));
            this.RaisePropertyChanged(nameof(this.HasStepMeasured));
            this.RaisePropertyChanged(nameof(this.HasStepConfirm));
        }

        #endregion
    }
}
