using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    internal sealed class CellPanelsCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellPanelsWebService machineCellPanelsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand applyCorrectionCommand;

        private Cell currentCell;

        private double? currentHeight;

        private CellPanel currentPanel;

        private int currentPanelNumber;

        private DelegateCommand goToCellHeightCommand;

        private DelegateCommand goToNextPanelCommand;

        private DelegateCommand goToPreviousPanelCommand;

        private bool hasReachedCellPosition;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingUp;

        private bool isWaitingForResponse;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private double? panelCorrection;

        private IEnumerable<CellPanel> panels;

        private PositioningProcedure procedureParameters;

        private double? stepValue;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public CellPanelsCheckViewModel(
            IMachineCellPanelsWebService machineCellPanelsWebService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineCellPanelsWebService = machineCellPanelsWebService ?? throw new ArgumentNullException(nameof(machineCellPanelsWebService));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
           this.applyCorrectionCommand
           ??
           (this.applyCorrectionCommand = new DelegateCommand(
               async () => await this.ApplyCorrectionAsync(),
               this.CanApplyCorrection));

        public Cell CurrentCell
        {
            get => this.currentCell;
            private
            set
            {
                if (this.SetProperty(ref this.currentCell, value))
                {
                    this.HasReachedCellPosition = false;
                }
            }
        }

        public double? CurrentHeight
        {
            get => this.currentHeight;
            private set => this.SetProperty(ref this.currentHeight, value);
        }

        public CellPanel CurrentPanel
        {
            get => this.currentPanel;
            private set
            {
                if (this.SetProperty(ref this.currentPanel, value))
                {
                    this.CurrentCell = this.CurrentPanel?.Cells
                        .OrderBy(c => c.Position)
                        .FirstOrDefault();
                }
            }
        }

        public int CurrentPanelNumber
        {
            get => this.currentPanelNumber;
            private set
            {
                if (this.SetProperty(ref this.currentPanelNumber, value))
                {
                    this.CurrentPanel = this.Panels?.ElementAtOrDefault(value - 1);
                }
            }
        }

        public double? Displacement
        {
            get => this.panelCorrection;
            private set => this.SetProperty(ref this.panelCorrection, value);
        }

        public ICommand GoToCellHeightCommand =>
           this.goToCellHeightCommand
           ??
           (this.goToCellHeightCommand = new DelegateCommand(
               this.GoToCellHeight,
               this.CanGoToCellHeight));

        public ICommand GoToNextPanelCommand =>
           this.goToNextPanelCommand
           ??
           (this.goToNextPanelCommand = new DelegateCommand(
               this.GoToNextPanel,
               this.CanGoToNextPanel));

        public ICommand GoToPreviousPanelCommand =>
          this.goToPreviousPanelCommand
          ??
          (this.goToPreviousPanelCommand = new DelegateCommand(
              this.GoToPreviousPanel,
              this.CanGoToPreviousPanel));

        public bool HasReachedCellPosition
        {
            get => this.hasReachedCellPosition;
            private set => this.SetProperty(ref this.hasReachedCellPosition, value);
        }

        public bool IsElevatorMovingDown
        {
            get => this.isElevatorMovingDown;
            private set => this.SetProperty(ref this.isElevatorMovingDown, value);
        }

        public bool IsElevatorMovingToCell
        {
            get => this.isElevatorMovingToCell;
            private set => this.SetProperty(ref this.isElevatorMovingToCell, value);
        }

        public bool IsElevatorMovingUp
        {
            get => this.isElevatorMovingUp;
            private set => this.SetProperty(ref this.isElevatorMovingUp, value);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }
                }
            }
        }

        public ICommand MoveDownCommand =>
           this.moveDownCommand
           ??
           (this.moveDownCommand = new DelegateCommand(
               this.MoveDown,
               this.CanMoveUpOrDown));

        public ICommand MoveUpCommand =>
           this.moveUpCommand
           ??
           (this.moveUpCommand = new DelegateCommand(
               this.MoveUp,
               this.CanMoveUpOrDown));

        public IEnumerable<CellPanel> Panels
        {
            get => this.panels;
            private set
            {
                var panels = value
                    .OrderBy(p => p.Side)
                    .ThenBy(p => p.Cells.Min(c => c.Position))
                    .ToArray();

                if (this.SetProperty(ref this.panels, panels))
                {
                    this.CurrentPanelNumber = 1;
                }
            }
        }

        public double? StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(this.Stop, this.CanStoped));

        private bool IsElevatorMoving
        {
            get => this.isElevatorMovingUp || this.isElevatorMovingDown || this.isElevatorMovingToCell;
            set
            {
                this.IsElevatorMovingUp = false;
                this.IsElevatorMovingDown = false;
                this.IsElevatorMovingToCell = false;
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
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnCurrentPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.IsBackNavigationAllowed = true;

            try
            {
                this.Panels = await this.machineCellPanelsWebService.GetAllAsync();

                this.CurrentHeight = await this.machineElevatorWebService.GetVerticalPositionAsync();

                this.procedureParameters = await this.machineCellPanelsWebService.GetProcedureParametersAsync();

                this.StepValue = this.procedureParameters.Step;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var hasChanged = base.SetProperty(ref storage, value, propertyName);
            if (hasChanged)
            {
                this.RaiseCanExecuteChanged();
            }

            return hasChanged;
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineCellPanelsWebService.UpdateHeightAsync(
                    this.CurrentCell.Id,
                    this.CurrentCell.Position + this.Displacement.Value);

                var currentPanelNumber = this.CurrentPanelNumber;
                this.Panels = await this.machineCellPanelsWebService.GetAllAsync();
                this.CurrentPanelNumber = currentPanelNumber;

                this.Displacement = null;
                this.HasReachedCellPosition = true;

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

        private bool CanApplyCorrection()
        {
            return !this.IsWaitingForResponse
                &&
                this.HasReachedCellPosition
                &&
                this.Displacement.HasValue
                &&
                this.Displacement != 0;
        }

        private bool CanGoToCellHeight()
        {
            return
                this.CurrentCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanGoToNextPanel()
        {
            return
                this.Panels != null
                &&
                this.CurrentPanelNumber < this.Panels.Count()
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanGoToPreviousPanel()
        {
            return
                this.Panels != null
                &&
                this.CurrentPanelNumber > 1
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanMoveUpOrDown()
        {
            return
                this.StepValue.HasValue
                &&
                !this.IsWaitingForResponse
                &&
                this.HasReachedCellPosition
                &&
                !this.IsElevatorMoving;
        }

        private bool CanStoped()
        {
            return this.IsElevatorMoving
                &&
                !this.IsWaitingForResponse;
        }

        private void GoToCellHeight()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsElevatorMovingToCell = true;
                this.HasReachedCellPosition = false;

                this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.CurrentCell.Position,
                    this.procedureParameters.FeedRate,
                    false);

                this.HasReachedCellPosition = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToCell = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void GoToNextPanel()
        {
            try
            {
                this.CurrentPanelNumber++;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void GoToPreviousPanel()
        {
            try
            {
                this.CurrentPanelNumber--;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void MoveDown()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsElevatorMovingDown = true;

                this.machineElevatorWebService.MoveVerticalOfDistanceAsync(-this.StepValue.Value);

                this.Displacement = (this.Displacement ?? 0) + this.StepValue.Value;
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

        private void MoveUp()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsElevatorMovingUp = true;

                this.machineElevatorWebService.MoveVerticalOfDistanceAsync(this.StepValue.Value);

                this.Displacement = (this.Displacement ?? 0) - this.StepValue.Value;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingUp = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnCurrentPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            this.CurrentHeight = message.Data?.CurrentPosition ?? this.CurrentHeight;

            if (message.IsErrored())
            {
                this.IsElevatorMoving = false;

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }
            else if (message.IsNotRunning())
            {
                this.IsElevatorMoving = false;

                if (message.Data.MovementType == CommonUtils.Messages.Enumerations.MovementType.Absolute
                    &&
                    message.Status != CommonUtils.Messages.Enumerations.MessageStatus.OperationStop
                    &&
                    message.Status != CommonUtils.Messages.Enumerations.MessageStatus.OperationRunningStop)
                {
                    this.HasReachedCellPosition = true;

                    this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsCellPosition);
                }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.goToCellHeightCommand?.RaiseCanExecuteChanged();
            this.goToNextPanelCommand?.RaiseCanExecuteChanged();
            this.goToPreviousPanelCommand?.RaiseCanExecuteChanged();
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
        }

        private void Stop()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.machineElevatorWebService.StopAsync();
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
