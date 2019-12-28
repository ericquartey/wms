using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class ErrorLoadunitMissingViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private bool canInputLoadingUnitId;

        private IEnumerable<Cell> cells;

        private SubscriptionToken cellsToken;

        private MachineError error;

        private string errorTime;

        private int? inputLoadingUnitId;

        private IEnumerable<LoadingUnit> loadunits;

        private SubscriptionToken loadunitsToken;

        private DelegateCommand markAsResolvedCommand;

        private DelegateCommand moveLoadunitCommand;

        private LoadingUnit selectedLoadingUnit;

        #endregion

        #region Constructors

        public ErrorLoadunitMissingViewModel(
            IMachineElevatorWebService machineElevatorWebService,
            IMachineErrorsWebService machineErrorsWebService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
        }

        #endregion

        #region Properties

        public bool CanInputLoadingUnitId
        {
            get => this.canInputLoadingUnitId;
            private set => this.SetProperty(ref this.canInputLoadingUnitId, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineError Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value);
        }

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

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

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set => this.SetProperty(ref this.selectedLoadingUnit, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Error = null;

            if (this.loadunitsToken != null)
            {
                this.loadunitsToken.Dispose();
                this.loadunitsToken = null;
            }

            if (this.cellsToken != null)
            {
                this.cellsToken.Dispose();
                this.cellsToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            this.IsBackNavigationAllowed = false;

            await base.OnAppearedAsync();

            this.Error = await this.machineErrorsWebService.GetCurrentAsync();

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

            this.IsWaitingForResponse = false;

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await base.OnMachineStatusChangedAsync(e);
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
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                ;
        }

        private bool CanMarkAsResolved()
        {
            return
                this.Error != null &&
                (this.Error.Code == (int)MachineErrorCode.MachineManagerErrorLoadingUnitMissingOnElevator ||
                 (this.Error.Code == (int)MachineErrorCode.MachineManagerErrorLoadingUnitMissingOnBay1 && this.Error.BayNumber == this.MachineService.BayNumber) ||
                 (this.Error.Code == (int)MachineErrorCode.MachineManagerErrorLoadingUnitMissingOnBay2 && this.Error.BayNumber == this.MachineService.BayNumber) ||
                 (this.Error.Code == (int)MachineErrorCode.MachineManagerErrorLoadingUnitMissingOnBay3 && this.Error.BayNumber == this.MachineService.BayNumber)) &&
                !this.IsWaitingForResponse;
        }

        private bool CanMoveLoadunit()
        {
            return
                this.CanMarkAsResolved() &&
                this.SelectedLoadingUnit?.Cell != null;
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
            if (this.Error is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                // await this.machineErrorsWebService.ResolveAsync(this.Error.Id);
                await this.machineErrorsWebService.ResolveAllAsync();

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
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

        private async Task MoveLoadunitAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.SelectedLoadingUnit.Cell is null)
                {
                    await this.machineElevatorWebService.MoveToFreeCellAsync(
                        this.SelectedLoadingUnit.Id,
                        performWeighting: true,
                        computeElongation: true);
                }
                else
                {
                    await this.machineElevatorWebService.MoveToCellAsync(
                        this.SelectedLoadingUnit.Cell.Id,
                        performWeighting: true,
                        computeElongation: true);
                }
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
