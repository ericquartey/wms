using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseItemOperationMainViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private readonly IMissionsDataService missionDataService;

        private DelegateCommand abortOperationCommand;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCommand;

        private int? inputQuantity;

        private bool isBusyAbortingOperation;

        private bool isBusyConfirmingOperation;

        private bool isWaitingForResponse;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private TrayControlCompartment selectedCompartment;

        private DelegateCommand showDetailsCommand;

        #endregion

        #region Constructors

        public BaseItemOperationMainViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService)
            : base(wmsImagesProvider, missionsDataService, bayManager, missionOperationsService)
        {
            this.missionDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));

            this.CompartmentColoringFunction = (c, selectedCompartment) => "#00FF00";
        }

        #endregion

        #region Properties

        public ICommand AbortOperationCommand =>
            this.abortOperationCommand
            ??
            (this.abortOperationCommand = new DelegateCommand(
                async () => await this.AbortOperationAsync(),
                this.CanAbortOperation));

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));

        public override EnableMask EnableMask => EnableMask.Any;

        public int? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyAbortingOperation
        {
            get => this.isBusyAbortingOperation;
            set => this.SetProperty(ref this.isBusyAbortingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            set => this.SetProperty(ref this.isBusyConfirmingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value, this.RaiseCanExecuteChanged);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public ICommand ShowDetailsCommand =>
            this.showDetailsCommand
            ??
            (this.showDetailsCommand = new DelegateCommand(this.ShowOperationDetails));

        #endregion

        #region Methods

        public async Task ConfirmOperationAsync()
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;

                await this.MissionOperationsService.CompleteCurrentMissionOperationAsync(this.InputQuantity.Value);

                await this.CheckOtherOperationsOnSameMissionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusyConfirmingOperation = false;
                this.IsWaitingForResponse = true;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.GetLoadingUnitDetails();
        }

        protected abstract void ShowOperationDetails();

        private async Task AbortOperationAsync()
        {
            try
            {
                this.IsBusyAbortingOperation = true;
                this.IsWaitingForResponse = true;

                // TODO show prompt dialog "are you sure?"
                var success = await this.MissionOperationsService.AbortCurrentMissionOperationAsync();
                if (success)
                {
                    this.NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = null;
                this.IsBusyAbortingOperation = false;
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanAbortOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CanConfirmOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0;
        }

        private async Task CheckOtherOperationsOnSameMissionAsync()
        {
            try
            {
                var mission = await this.missionDataService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);
                if (mission.Operations?.Any(o => o.Status == MissionOperationStatus.New) == true)
                {
                    this.IsEnabled = false;
                    this.InputQuantity = null;
                    this.ItemImage = null;
                }
                else
                {
                    this.NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void GetLoadingUnitDetails()
        {
            try
            {
                System.Diagnostics.Debug.Assert(
                    this.MissionOperationsService.CurrentMission != null,
                    "This view model should not be opened if there is no current mission");

                this.Compartments = this.MapCompartments(this.Mission.LoadingUnit.Compartments);
                this.LoadingUnitWidth = this.Mission.LoadingUnit.Width;
                this.LoadingUnitDepth = this.Mission.LoadingUnit.Depth;
                this.SelectedCompartment = this.Compartments.SingleOrDefault(c =>
                    c.Id == this.MissionOperationsService.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentMissionInfo> compartmentsFromMission)
        {
            return compartmentsFromMission
                .Where(c =>
                    c.Width.HasValue
                    ||
                    c.Depth.HasValue
                    ||
                    c.XPosition.HasValue
                    ||
                    c.YPosition.HasValue)
                .Select(c => new TrayControlCompartment
                {
                    Depth = c.Depth.Value,
                    Id = c.Id,
                    Width = c.Width.Value,
                    XPosition = c.XPosition.Value,
                    YPosition = c.YPosition.Value,
                });
        }

        private void RaiseCanExecuteChanged()
        {
            this.abortOperationCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.showDetailsCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
