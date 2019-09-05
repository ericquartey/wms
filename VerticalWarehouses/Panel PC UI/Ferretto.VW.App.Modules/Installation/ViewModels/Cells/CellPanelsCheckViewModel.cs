using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CellPanelsCheckViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellPanelsService machineCellPanelsService;

        private readonly IMachineElevatorService machineElevatorService;

        private DelegateCommand applyCorrectionCommand;

        private Cell currentCell;

        private decimal? currentHeight;

        private CellPanel currentPanel;

        private int currentPanelIndex;

        private DelegateCommand goToCellHeightCommand;

        private DelegateCommand goToNextPanelCommand;

        private bool hasReachedCellPosition;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingUp;

        private bool isWaitingForResponse;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        private decimal? panelCorrection;

        private IEnumerable<CellPanel> panels;

        private decimal? stepValue;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public CellPanelsCheckViewModel(
            IMachineCellPanelsService machineCellPanelsService,
            IMachineElevatorService machineElevatorService)
            : base(Services.PresentationMode.Installer)
        {
            if (machineCellPanelsService is null)
            {
                throw new ArgumentNullException(nameof(machineCellPanelsService));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            this.machineCellPanelsService = machineCellPanelsService;
            this.machineElevatorService = machineElevatorService;
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

        public decimal? CurrentHeight
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
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ShowNotification(string.Empty, Services.Models.NotificationSeverity.Clear);
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

        public decimal? PanelCorrection
        {
            get => this.panelCorrection;
            private set => this.SetProperty(ref this.panelCorrection, value);
        }

        public IEnumerable<CellPanel> Panels
        {
            get => this.panels;
            private set
            {
                if (this.SetProperty(ref this.panels, value))
                {
                    this.currentPanelIndex = 0;

                    this.CurrentPanel = this.Panels.FirstOrDefault();
                }
            }
        }

        public decimal? StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value);
        }

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
            await base.OnNavigatedAsync();

            this.subscriptionToken = this.EventAggregator
             .GetEvent<NotificationEventUI<PositioningMessageData>>()
             .Subscribe(
                 message => this.OnCurrentHeightChanged(message),
                 ThreadOption.UIThread,
                 false);

            this.IsBackNavigationAllowed = true;

            try
            {
                var panels = await this.machineCellPanelsService
                    .GetAllAsync();

                this.Panels = panels
                    .OrderBy(p => p.Side)
                    .ThenBy(p => p.Cells.Min(c => c.Position));
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
                await this.machineCellPanelsService.UpdateHeightAsync(
                    this.CurrentCell.Id,
                    this.CurrentCell.Position + this.PanelCorrection.Value);

                var currentpanelId = this.CurrentPanel.Id;
                this.Panels = await this.machineCellPanelsService.GetAllAsync();
                this.CurrentPanel = this.Panels.SingleOrDefault(p => p.Id == currentpanelId);
                this.HasReachedCellPosition = true;

                this.ShowNotification("Correzione altezza applicata.");
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
                this.PanelCorrection.HasValue
                &&
                this.PanelCorrection != 0;
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
                this.Panels.LastOrDefault() != this.CurrentPanel
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

        private void GoToCellHeight()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsElevatorMovingToCell = true;
                this.HasReachedCellPosition = false;

                this.machineElevatorService.MoveToVerticalPositionAsync(this.CurrentCell.Position, FeedRateCategory.PanelHeightCheck);

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
                this.currentPanelIndex++;
                this.CurrentPanel = this.Panels.ElementAtOrDefault(this.currentPanelIndex);
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

                this.machineElevatorService.MoveVerticalOfDistanceAsync(-this.StepValue.Value);

                this.PanelCorrection = this.PanelCorrection.HasValue
                    ? this.PanelCorrection + this.StepValue.Value
                    : this.StepValue.Value;
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

                this.machineElevatorService.MoveVerticalOfDistanceAsync(this.StepValue.Value);

                this.PanelCorrection = this.PanelCorrection.HasValue
                    ? this.PanelCorrection - this.StepValue.Value
                    : this.StepValue.Value;
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

        private void OnCurrentHeightChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            this.CurrentHeight = message?.Data?.CurrentPosition;

            if (message is null || message.Data is null)
            {
                return;
            }

            switch (message?.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsElevatorMoving = false;

                        if (message.Data.MovementType == CommonUtils.Messages.Enumerations.MovementType.Absolute)
                        {
                            this.HasReachedCellPosition = true;
                            this.ShowNotification(
                                "Altezza cella raggiunta.");
                        }

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsElevatorMoving = false;

                        this.ShowNotification(
                            "Procedura di posizionamento interrotta.",
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.goToCellHeightCommand?.RaiseCanExecuteChanged();
            this.goToNextPanelCommand?.RaiseCanExecuteChanged();
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
