﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum CellPanelsCheckStep
    {
        Inizialize,

        MeasuredFront,

        MeasuredBack,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class CellPanelsCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private int lastPanel;

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

        private CellPanelsCheckStep currentStep;

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

        private SubscriptionToken themeChangedToken;

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
                        .Where(w => w.BlockLevel.Equals(BlockLevel.None))
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

                    this.CurrentPanelMinValue =
                        this.CurrentPanel.Cells
                            .Where(w => w.BlockLevel.Equals(BlockLevel.None))
                            .DefaultIfEmpty(new Cell())
                            .Min(m => m.Id);
                    this.CurrentPanelMaxValue =
                        this.CurrentPanel.Cells
                            .Where(w => w.BlockLevel.Equals(BlockLevel.None))
                            .DefaultIfEmpty(new Cell())
                            .Max(m => m.Id);

                    this.CurrentCellId = this.CurrentPanelMinValue;
                }
            }
        }

        public CellPanelsCheckStep CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
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

        public bool HasStepInitialize => this.currentStep is CellPanelsCheckStep.Inizialize;

        public bool HasStepMeasuredBack => this.currentStep is CellPanelsCheckStep.MeasuredBack;

        public bool HasStepMeasuredFront => this.currentStep is CellPanelsCheckStep.MeasuredFront;

        public bool IsCanStartPosition => this.CanBaseExecute();

        public bool IsCanStepValue => this.CanBaseExecute() && this.CurrentCellId > 0;

        public IEnumerable<CellPanel> Panels
        {
            get => this.panels;
            private set
            {
                var panels = value
                    .ToList()
                    .Where(w => w.Cells.Any(c => c.BlockLevel.Equals(BlockLevel.None)))
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
                            !this.CurrentPanel.Cells
                            .Where(w => w.BlockLevel.Equals(BlockLevel.None))
                            .Any(a => a.Id == this.CurrentCellId))
                        {
                            return Localized.Get("InstallationApp.CellNotValid");
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

            if (this.themeChangedToken != null)
            {
                this.EventAggregator.GetEvent<ThemeChangedPubSubEvent>().Unsubscribe(this.themeChangedToken);
                this.themeChangedToken?.Dispose();
                this.themeChangedToken = null;
            }

                this.lastPanel = this.currentPanelNumber;
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();

            if (this.lastPanel != 0)
            {
                this.CurrentPanelNumber = this.lastPanel;
            }

            if (this.CurrentStep == CellPanelsCheckStep.MeasuredBack && this.CurrentPanel.Side != WarehouseSide.Back)
            {
                this.CurrentStep = CellPanelsCheckStep.MeasuredFront;
                this.RaisePropertyChanged(nameof(this.CurrentStep));
            }
            else if(this.CurrentStep == CellPanelsCheckStep.MeasuredFront && this.CurrentPanel.Side != WarehouseSide.Front)
            {
                this.CurrentStep = CellPanelsCheckStep.MeasuredBack;
                this.RaisePropertyChanged(nameof(this.CurrentStep));
            }
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
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.onGoToCell = false;
                }
            }
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
                    VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"),
                    Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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
                   this.CurrentCellId > 0 &&
                   this.StepValue != 0;
        }

        private bool CanGoToCellHeight()
        {
            return this.CurrentCell != null &&
                   !this.IsMoving &&
                   (Convert.ToInt32(this.CurrentCell.Position) != Convert.ToInt32(this.MachineStatus?.ElevatorVerticalPosition ?? 0D) ||
                    this.CurrentCell.Id != this.MachineStatus?.LogicalPositionId);
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
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private WarehouseSide GetNextPanelSide(int count)
        {
            return this.Panels.ElementAtOrDefault(count).Side;
        }

        private WarehouseSide GetPrevPanelSide(int count)
        {
            return this.Panels.ElementAtOrDefault(count - 2).Side;
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
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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
            this.CurrentCell = this.CurrentPanel?.Cells
                .Where(w => w.BlockLevel.Equals(BlockLevel.None))
                .FirstOrDefault(f => f.Id == this.currentCellId);
            this.RaiseCanExecuteChanged();
        }

        private void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case CellPanelsCheckStep.Inizialize:
                    if (e.Next)
                    {
                        this.CurrentStep = CellPanelsCheckStep.MeasuredFront;
                    }

                    break;

                case CellPanelsCheckStep.MeasuredFront:
                    if (e.Next && this.CurrentPanelNumber < this.Panels.Count() && this.GetNextPanelSide(this.CurrentPanelNumber) == WarehouseSide.Back)
                    {
                        this.CurrentPanelNumber++;

                        this.CurrentStep = CellPanelsCheckStep.MeasuredBack;
                    }
                    else if (e.Next && this.CurrentPanelNumber < this.Panels.Count())
                    {
                        this.CurrentPanelNumber++;

                        this.RaiseCanExecuteChanged();
                    }
                    else if (e.Back && this.CurrentPanelNumber > 1 && this.GetNextPanelSide(this.CurrentPanelNumber) == WarehouseSide.Back)
                    {
                        this.CurrentPanelNumber--;

                        this.CurrentStep = CellPanelsCheckStep.MeasuredBack;
                    }
                    else if (e.Back && this.CurrentPanelNumber > 1)
                    {
                        this.CurrentPanelNumber--;

                        this.RaiseCanExecuteChanged();
                    }
                    else if (e.Back && this.CurrentPanelNumber == 1)
                    {
                        this.CurrentStep = CellPanelsCheckStep.Inizialize;
                    }

                    this.Displacement = 0;

                    break;

                case CellPanelsCheckStep.MeasuredBack:
                    if (e.Next && this.CurrentPanelNumber < this.Panels.Count() && this.GetNextPanelSide(this.CurrentPanelNumber) == WarehouseSide.Front)
                    {
                        this.CurrentPanelNumber++;

                        this.CurrentStep = CellPanelsCheckStep.MeasuredFront;
                    }
                    else if (e.Next && this.CurrentPanelNumber < this.Panels.Count())
                    {
                        this.CurrentPanelNumber++;

                        this.RaiseCanExecuteChanged();
                    }
                    else if (e.Back && this.GetPrevPanelSide(this.CurrentPanelNumber) == WarehouseSide.Front)
                    {
                        this.CurrentPanelNumber--;

                        this.CurrentStep = CellPanelsCheckStep.MeasuredFront;
                    }
                    else if (e.Back && this.CurrentPanelNumber > 1)
                    {
                        this.CurrentPanelNumber--;

                        this.RaiseCanExecuteChanged();
                    }

                    this.Displacement = 0;

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        private async Task Stop()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.MachineService.StopMovingByAllAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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

            this.themeChangedToken = this.themeChangedToken
               ?? this.EventAggregator
                   .GetEvent<ThemeChangedPubSubEvent>()
                   .Subscribe(
                       (m) =>
                       {
                           this.RaisePropertyChanged(nameof(this.HasStepInitialize));
                           this.RaisePropertyChanged(nameof(this.HasStepMeasuredFront));
                           this.RaisePropertyChanged(nameof(this.HasStepMeasuredBack));
                       },
                       ThreadOption.UIThread,
                       false);
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case CellPanelsCheckStep.Inizialize:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CellPanelsCheckStep.MeasuredFront:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case CellPanelsCheckStep.MeasuredBack:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;
            }

            this.ShowAbortStep(true, true);

            this.RaisePropertyChanged(nameof(this.HasStepInitialize));
            this.RaisePropertyChanged(nameof(this.HasStepMeasuredFront));
            this.RaisePropertyChanged(nameof(this.HasStepMeasuredBack));
        }

        #endregion
    }
}
