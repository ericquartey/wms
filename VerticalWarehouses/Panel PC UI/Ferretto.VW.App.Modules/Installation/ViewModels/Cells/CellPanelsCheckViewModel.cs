using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
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
    [Warning(WarningsArea.Installation)]
    internal sealed class CellPanelsCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineCellPanelsWebService machineCellPanelsWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand applyCorrectionCommand;

        private Cell currentCell;

        private int currentCellId;

        private string currentError;

        private double? currentHeight;

        private CellPanel currentPanel;

        private int currentPanelMaxValue;

        private int currentPanelMinValue;

        private int currentPanelNumber;

        private double displacement;

        private DelegateCommand displacementCommand;

        private DelegateCommand goToCellHeightCommand;

        private bool onGoToCell;

        private IEnumerable<CellPanel> panels;

        private int panelsCheck;

        private double? panelsCheckPercent;

        private SubscriptionToken stepChangedToken;

        private double stepValue;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public CellPanelsCheckViewModel(
            IMachineCellPanelsWebService machineCellPanelsWebService,
            IMachineElevatorWebService machineElevatorWebService)
            : base(PresentationMode.Installer)
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
            set => this.SetProperty(ref this.currentCell, value);
        }

        public int CurrentCellId
        {
            get => this.currentCellId;
            set => this.SetProperty(ref this.currentCellId, value, this.OnCurrentCellIdChanged);
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

        public bool CurrentPanelIsChecked => this.CurrentPanel?.IsChecked ?? false;

        public int CurrentPanelMaxValue
        {
            get => this.currentPanelMaxValue;
            set => this.SetProperty(ref this.currentPanelMaxValue, value);
        }

        public int CurrentPanelMinValue
        {
            get => this.currentPanelMinValue;
            set => this.SetProperty(ref this.currentPanelMinValue, value);
        }

        public int CurrentPanelNumber
        {
            get => this.currentPanelNumber;
            private set
            {
                if (this.SetProperty(ref this.currentPanelNumber, value))
                {
                    this.CurrentPanel = this.Panels?.ElementAtOrDefault(value - 1);

                    this.CurrentPanelMinValue = this.CurrentPanel.Cells.Min(m => m.Id);
                    this.CurrentPanelMaxValue = this.CurrentPanel.Cells.Max(m => m.Id);

                    this.CurrentCellId = this.CurrentPanelMinValue;
                }
            }
        }

        public double Displacement
        {
            get => this.displacement;
            private set => this.SetProperty(ref this.displacement, value);
        }

        public ICommand DisplacementCommand =>
            this.displacementCommand
            ??
            (this.displacementCommand = new DelegateCommand(
                async () => await this.DisplacementCommandAsync(),
                this.CanDisplacementCommand));

        public string Error => string.Join(
            Environment.NewLine,
            this.GetType().GetProperties()
                .Select(p => this[p.Name])
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s)));

        public ICommand GoToCellHeightCommand =>
           this.goToCellHeightCommand
           ??
           (this.goToCellHeightCommand = new DelegateCommand(
               async () => await this.GoToCellHeightAsync(),
               this.CanGoToCellHeight));

        public bool IsCanStartPosition => this.CanBaseExecute();

        public bool IsCanStepValue => this.CanBaseExecute();

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
                    this.PanelsCheck = this.panels.Count(c => c.IsChecked);
                    this.PanelsCheckPercent = (double)this.panels.Count(c => c.IsChecked) / (double)this.panels.Count() * 100.0;

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int PanelsCheck
        {
            get => this.panelsCheck;
            private set => this.SetProperty(ref this.panelsCheck, value);
        }

        public double? PanelsCheckPercent
        {
            get => this.panelsCheckPercent;
            private set => this.SetProperty(ref this.panelsCheckPercent, value);
        }

        public double StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(async () => await this.Stop(), this.CanStop));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(this.CurrentCellId):
                        if (this.CurrentPanel == null ||
                            !this.CurrentPanel.Cells.Any(a => a.Id == this.CurrentCellId))
                        {
                            return InstallationApp.CellNotValid;
                        }

                        break;

                    case nameof(this.StepValue):
                        break;
                }

                if (this.IsVisible && string.IsNullOrEmpty(this.currentError))
                {
                    //this.ClearNotifications();
                }

                return null;
            }
        }

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

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await this.SensorsService.RefreshAsync(true);

                this.Panels = await this.machineCellPanelsWebService.GetAllAsync();

                this.CurrentHeight = this.MachineStatus.ElevatorVerticalPosition;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);

            // Se mi sono posizionato sulla cella richiesta attivo l'automazione che setto il pannello come controllato
            if (!this.IsMoving &&
                this.MachineStatus.MessageStatus == CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd &&
                this.CurrentPanel != null &&
                !this.CurrentPanel.IsChecked &&
                this.onGoToCell)
            {
                try
                {
                    await this.machineCellPanelsWebService.UpdateHeightAsync(this.CurrentPanel.Id, this.Displacement);

                    var currentPanelNumber = this.CurrentPanelNumber;
                    this.Panels = await this.machineCellPanelsWebService.GetAllAsync();
                    this.CurrentPanelNumber = currentPanelNumber;
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.onGoToCell = false;
                }
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            if (e.Next && this.CurrentPanelNumber < this.Panels.Count())
            {
                this.CurrentPanelNumber++;

                this.RaiseCanExecuteChanged();
            }
            else if (e.Back && this.CurrentPanelNumber > 1)
            {
                this.CurrentPanelNumber--;

                this.RaiseCanExecuteChanged();
            }

            this.Displacement = 0;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsCanStartPosition));
            this.RaisePropertyChanged(nameof(this.IsCanStepValue));
            this.RaisePropertyChanged(nameof(this.CurrentPanel));
            this.RaisePropertyChanged(nameof(this.CurrentPanelIsChecked));

            this.displacementCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.goToCellHeightCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineCellPanelsWebService.UpdateHeightAsync(this.CurrentPanel.Id, this.Displacement);

                var currentPanelNumber = this.CurrentPanelNumber;
                this.Panels = await this.machineCellPanelsWebService.GetAllAsync();
                this.CurrentPanelNumber = currentPanelNumber;

                this.Displacement = 0;

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
            return !this.IsMoving;
        }

        private bool CanBaseExecute()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   !this.SensorsService.IsHorizontalInconsistentBothLow &&
                   !this.SensorsService.IsHorizontalInconsistentBothHigh &&
                   !this.SensorsService.IsLoadingUnitOnElevator;
        }

        private bool CanDisplacementCommand()
        {
            return this.CanBaseExecute() &&
                   this.StepValue != 0;
        }

        private bool CanGoToCellHeight()
        {
            return this.CurrentCell != null &&
                   !this.IsMoving &&
                   Convert.ToInt32(this.CurrentCell.Position) != Convert.ToInt32(this.MachineStatus?.ElevatorVerticalPosition ?? 0D);
        }

        private bool CanStop()
        {
            return this.IsMoving;
        }

        private async Task DisplacementCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveVerticalOfDistanceAsync(this.StepValue);

                this.Displacement += this.StepValue;
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

        private async Task GoToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorWebService.MoveToCellAsync(
                    this.CurrentCell.Id,
                    computeElongation: false,
                    performWeighting: false);

                this.Displacement = 0;

                this.onGoToCell = true;
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

        private void OnCurrentCellIdChanged()
        {
            this.CurrentCell = this.CurrentPanel?.Cells.FirstOrDefault(f => f.Id == this.currentCellId);
            this.RaiseCanExecuteChanged();
        }

        private async Task Stop()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.MachineService.StopMovingByAllAsync();
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

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);
        }

        private void UpdateStatusButtonFooter()
        {
            this.ShowPrevStepSinglePage(true, true);
            this.ShowNextStepSinglePage(true, true);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
