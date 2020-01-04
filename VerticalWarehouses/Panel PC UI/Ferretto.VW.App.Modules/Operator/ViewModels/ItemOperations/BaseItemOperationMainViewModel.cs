using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationMainViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private MAS.AutomationService.Contracts.Bay bay;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCanceledCommand;

        private DelegateCommand confirmOperationCommand;

        private double? inputQuantity;

        private bool isBusyAbortingOperation;

        private bool isBusyConfirmingOperation;

        private bool isOperationCanceled;

        private bool isOperationConfirmed;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private SubscriptionToken missionToken;

        private TrayControlCompartment selectedCompartment;

        private DelegateCommand showDetailsCommand;

        #endregion

        #region Constructors

        public BaseItemOperationMainViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsWmsWebService missionsWmsWebService,
            IBayManager bayManager,
            IEventAggregator eventAggregator,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, bayManager, missionOperationsService, dialogService)
        {
            this.eventAggregator = eventAggregator;

            this.CompartmentColoringFunction = (compartment, selectedCompartment) => compartment == selectedCompartment ? "#0288f7" : "#444444";
        }

        #endregion

        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCanceledCommand =>
            this.confirmOperationCanceledCommand
            ??
            (this.confirmOperationCanceledCommand = new DelegateCommand(
                async () => await this.ConfirmOperationCanceledAsync(),
                this.CanConfirmOperationCanceled));

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));

        public override EnableMask EnableMask => EnableMask.Any;

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBaySideBack => this.bay?.Side == MAS.AutomationService.Contracts.WarehouseSide.Back;

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

        public bool IsOperationCanceled
        {
            get => this.isOperationCanceled;
            set => this.SetProperty(ref this.isOperationCanceled, value);
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

        public virtual bool CanConfirmOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                !this.isOperationConfirmed
                &&
                !this.isOperationCanceled
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value == this.MissionOperation.RequestedQuantity;
        }

        public virtual bool CanConfirmOperationCanceled()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.isOperationConfirmed
                &&
                this.isOperationCanceled;
        }

        public async Task ConfirmOperationAsync()
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                await this.MissionOperationsService.CompleteCurrentAsync(this.InputQuantity.Value);

                this.isOperationConfirmed = true;

                this.ShowNotification(Resources.OperatorApp.OperationConfirmed);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
            }
        }

        public async Task ConfirmOperationCanceledAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                await this.MissionOperationsService.CancelCurrentAsync();

                this.ShowNotification(Resources.OperatorApp.OperationCancelledConfirmed);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        public override void Disappear()
        {
            this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>().Unsubscribe(this.missionToken);
            this.missionToken?.Dispose();
            this.missionToken = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = false;
            this.IsBusyAbortingOperation = false;
            this.IsBusyConfirmingOperation = false;
            this.isOperationConfirmed = false;
            this.IsOperationCanceled = false;
            this.InputQuantity = null;
            this.SelectedCompartment = null;

            await base.OnAppearedAsync();

            this.bay = await this.BayManager.GetBayAsync();

            this.RaisePropertyChanged(nameof(this.IsBaySideBack));

            this.missionToken = this.missionToken
                ??
                this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnAssignedMissionOperationChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.GetLoadingUnitDetails();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.showDetailsCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCanceledCommand?.RaiseCanExecuteChanged();
        }

        protected abstract void ShowOperationDetails();

        private static IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentMissionInfo> compartmentsFromMission)
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

        private void GetLoadingUnitDetails()
        {
            if (this.Mission is null)
            {
                this.Compartments = null;
                this.SelectedCompartment = null;
                return;
            }

            try
            {
                this.Compartments = MapCompartments(this.Mission.LoadingUnit.Compartments);
                this.LoadingUnitWidth = this.Mission.LoadingUnit.Width;
                this.LoadingUnitDepth = this.Mission.LoadingUnit.Depth;
                this.SelectedCompartment = this.Compartments.SingleOrDefault(c =>
                    c.Id == this.MissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private string GetNoLongerOperationMessageByType()
        {
            var noLongerOperationMsg = string.Empty;
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    noLongerOperationMsg = OperatorApp.IfPickedItemsPutThemBackInTheOriginalCompartment;
                    break;

                case MissionOperationType.Put:
                    noLongerOperationMsg = OperatorApp.RemoveAnySpilledItemsFromCompartment;
                    break;

                case MissionOperationType.Inventory:
                    noLongerOperationMsg = OperatorApp.InventoryOperationCancelled;
                    break;

                default:
                    break;
            }

            return noLongerOperationMsg;
        }

        private void HideNavigationBack()
        {
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    this.IsBackNavigationAllowed = false;
                    break;

                case MissionOperationType.Put:
                    this.IsBackNavigationAllowed = false;
                    break;

                default:
                    break;
            }
        }

        private async Task OnAssignedMissionOperationChangedAsync(AssignedMissionOperationChangedEventArgs e)
        {
            if (this.isOperationConfirmed)
            {
                this.isOperationConfirmed = false;

                await this.RetrieveMissionOperationAsync();

                this.GetLoadingUnitDetails();
            }
            else
            {
                this.IsOperationCanceled = true;
                this.CanInputQuantity = false;
                var msg = this.GetNoLongerOperationMessageByType();
                this.DialogService.ShowMessage(msg, OperatorApp.OperationCancelled);
                this.ShowNotification(msg, Services.Models.NotificationSeverity.Warning);
                this.HideNavigationBack();
            }

            this.IsBusyConfirmingOperation = false;
            this.IsWaitingForResponse = false;
        }

        #endregion
    }
}
