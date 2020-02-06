using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Errors.ViewModels
{
    internal struct ErrorLoadunitMissingStepAutomaticMode
    {
    }

    internal struct ErrorLoadunitMissingStepLoadunit
    {
    }

    internal struct ErrorLoadunitMissingStepStart
    {
    }

    internal sealed class ErrorLoadunitMissingViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;
        private readonly IMachineModeWebService machineModeWebService;
        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private bool canInputLoadingUnitId;

        private IEnumerable<Cell> cells;

        private SubscriptionToken cellsToken;

        private object currentStep = default(ErrorLoadunitMissingStepStart);

        private MachineError error;

        private string errorTime;

        private int? inputLoadingUnitId;

        private IEnumerable<LoadingUnit> loadunits;

        private SubscriptionToken loadunitsToken;

        private DelegateCommand markAsResolvedCommand;

        private DelegateCommand moveLoadunitCommand;

        private DelegateCommand moveToNextCommand;

        private LoadingUnit selectedLoadingUnit;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        private string subtitleStepLoadunit;
        private DelegateCommand automaticCommand;

        #endregion

        #region Constructors

        public ErrorLoadunitMissingViewModel(
            IMachineModeWebService machineModeWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineErrorsWebService machineErrorsWebService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.machineModeWebService = machineModeWebService ?? throw new ArgumentNullException(nameof(machineModeWebService));

            this.CurrentStep = default(ErrorLoadunitMissingStepStart);
        }

        #endregion

        #region Properties

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public object CurrentStep
        {
            get => this.currentStep;
            set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineError Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value, () => this.OnErrorChanged(null));
        }

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public bool HasStepAutomaticMode => this.currentStep is ErrorLoadunitMissingStepAutomaticMode;

        public bool HasStepLoadunit => this.currentStep is ErrorLoadunitMissingStepLoadunit;

        public bool HasStepStart => this.currentStep is ErrorLoadunitMissingStepStart;

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set => this.SetProperty(ref this.inputLoadingUnitId, value, this.InputLoadingUnitIdPropertyChanged);
        }

        public bool IsMoving
        {
            get => this.MachineService?.MachineStatus?.IsMoving ?? true;
        }

        public override bool KeepAlive => false;

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanMarkAsResolved));

        public ICommand MoveLoadunitCommand =>
            this.moveLoadunitCommand
            ??
            (this.moveLoadunitCommand = new DelegateCommand(
                async () => await this.MoveLoadunitAsync(),
                this.CanMoveLoadunit));

        public ICommand MoveToNextCommand =>
            this.moveToNextCommand
            ??
            (this.moveToNextCommand = new DelegateCommand(
                () =>
                {
                    if (this.CurrentStep is ErrorLoadunitMissingStepStart)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepLoadunit);
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunit)
                    {
                        this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                    }
                    else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
                    {
                        throw new NotSupportedException();
                    }
                }));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set => this.SetProperty(ref this.selectedLoadingUnit, value);
        }


        private bool CanAutomaticCommand()
        {
            return !this.IsKeyboardOpened &&
                   !this.IsMoving &&
                   this.MachineService.MachineMode != MachineMode.Automatic;
        }


        public ICommand AutomaticCommand =>
            this.automaticCommand
            ??
            (this.automaticCommand = new DelegateCommand(
                async () => await this.AutomaticCommandAsync(),
                this.CanAutomaticCommand));


        private async Task AutomaticCommandAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineModeWebService.SetAutomaticAsync();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }




        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        public string SubtitleStepLoadunit
        {
            get => this.subtitleStepLoadunit;
            set => this.SetProperty(ref this.subtitleStepLoadunit, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Error = null;

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }

            if (this.loadunitsToken != null)
            {
                this.EventAggregator.GetEvent<LoadUnitsChangedPubSubEvent>().Unsubscribe(this.loadunitsToken);
                this.loadunitsToken.Dispose();
                this.loadunitsToken = null;
            }

            if (this.cellsToken != null)
            {
                this.EventAggregator.GetEvent<CellsChangedPubSubEvent>().Unsubscribe(this.cellsToken);
                this.cellsToken.Dispose();
                this.cellsToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.Error = await this.machineErrorsWebService.GetCurrentAsync();

                if (this.Error is null)
                {
                    await this.MarkAsResolvedAsync();
                }

                if (this.Error != null)
                {
                    if ((this.Error.Code == (int)MachineErrorCode.LoadUnitMissingOnElevator) ||
                        (this.Error.Code == (int)MachineErrorCode.LoadUnitSourceElevator))
                    {
                        this.SubtitleStepLoadunit = "Selezionare l'id del cassetto presente sull'elevatore.";
                    }
                    else
                    {
                        this.SubtitleStepLoadunit = "Selezionare l'id del cassetto/i presente/i in baia.";
                    }
                }
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

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);
            this.RaiseCanExecuteChanged();
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            if (this.CurrentStep is ErrorLoadunitMissingStepStart)
            {
                if (e.Next)
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunit);
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunit)
            {
                if (e.Next)
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepAutomaticMode);
                }
                else
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepStart);
                }
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
            {
                if (!e.Next)
                {
                    this.CurrentStep = default(ErrorLoadunitMissingStepLoadunit);
                }
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.CanInputLoadingUnitId =
                this.CanBaseExecute()
                &&
                this.loadunits != null
                &&
                this.cells != null;

            this.markAsResolvedCommand?.RaiseCanExecuteChanged();
            this.moveLoadunitCommand?.RaiseCanExecuteChanged();

            this.automaticCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();
            this.moveToNextCommand?.RaiseCanExecuteChanged();
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanMarkAsResolved()
        {
            return
                this.Error != null &&
                (this.Error.Code == (int)MachineErrorCode.LoadUnitMissingOnElevator ||
                 (this.Error.Code == (int)MachineErrorCode.LoadUnitMissingOnBay && this.Error.BayNumber == this.MachineService.BayNumber)) &&
                !this.IsWaitingForResponse;
        }

        private bool CanMoveLoadunit()
        {
            return
                this.CanMarkAsResolved();
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private void InputLoadingUnitIdPropertyChanged()
        {
            if (this.loadunits is null)
            {
                return;
            }

            this.SelectedLoadingUnit = this.inputLoadingUnitId == null
                ? null
                : this.loadunits.SingleOrDefault(c => c.Id == this.inputLoadingUnitId);

            this.RaiseCanExecuteChanged();
        }

        private async Task MarkAsResolvedAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAllAsync();

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveLoadunitAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.SelectedLoadingUnit.Cell is null)
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(
                        LoadingUnitLocation.Elevator,
                        null,
                        this.SelectedLoadingUnit.Id);
                }
                else
                {
                    await this.machineLoadingUnitsWebService.InsertLoadingUnitAsync(
                        LoadingUnitLocation.Elevator,
                        this.SelectedLoadingUnit.Cell.Id,
                        this.SelectedLoadingUnit.Id);
                }
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnErrorChanged(object state)
        {
            if (this.error is null)
            {
                this.ErrorTime = null;
                return;
            }

            var elapsedTime = DateTime.UtcNow - this.error.OccurrenceDate;
            if (elapsedTime.TotalMinutes < 1)
            {
                this.ErrorTime = Resources.VWApp.Now;
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Resources.VWApp.MinutesAgo, elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Resources.VWApp.HoursAgo, elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Resources.VWApp.DaysAgo, elapsedTime.TotalDays);
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
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
            this.loadunits = this.MachineService.Loadunits;
            this.loadunitsToken = this.loadunitsToken
                 ??
                 this.EventAggregator
                     .GetEvent<LoadUnitsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.loadunits = m.Loadunits,
                         ThreadOption.UIThread,
                         false);

            this.cells = this.MachineService.Cells;
            this.cellsToken = this.cellsToken
                 ??
                 this.EventAggregator
                     .GetEvent<CellsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.cells = m.Cells,
                         ThreadOption.UIThread,
                         false);

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
            if (this.CurrentStep is ErrorLoadunitMissingStepStart)
            {
                this.ShowPrevStepSinglePage(true, false);
                this.ShowNextStepSinglePage(true, true);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepLoadunit)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, true);
            }
            else if (this.CurrentStep is ErrorLoadunitMissingStepAutomaticMode)
            {
                this.ShowPrevStepSinglePage(true, !this.IsMoving);
                this.ShowNextStepSinglePage(true, true);
            }

            this.ShowAbortStep(true, !this.IsMoving);

            this.RaisePropertyChanged(nameof(this.HasStepStart));
            this.RaisePropertyChanged(nameof(this.HasStepLoadunit));
            this.RaisePropertyChanged(nameof(this.HasStepAutomaticMode));
        }

        #endregion
    }
}
