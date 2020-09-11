using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private bool canConfirm;

        private bool canConfirmOnEmpty;

        private DelegateCommand emptyOperationCommand;

        private string measureUnitTxt;

        #endregion

        #region Constructors

        public ItemPickViewModel(
            INavigationService navigationService,
            IOperatorNavigationService operatorNavigationService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(
                  navigationService,
                  operatorNavigationService,
                  loadingUnitsWebService,
                  itemsWebService,
                  compartmentsWebService,
                  missionOperationsWebService,
                  bayManager,
                  eventAggregator,
                  missionOperationsService,
                  dialogService)
        {
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPick.ToString();

        public bool CanConfirm
        {
            get => this.canConfirm;
            set => this.SetProperty(ref this.canConfirm, value);
        }

        public bool CanConfirmOnEmpty
        {
            get => this.canConfirmOnEmpty;
            set => this.SetProperty(ref this.canConfirmOnEmpty, value);
        }

        public ICommand EmptyOperationCommand =>
            this.emptyOperationCommand
            ??
            (this.emptyOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnEmptyCompartmentAsync(),
                this.CanPartiallyCompleteOnEmptyCompartment));

        public override EnableMask EnableMask => EnableMask.Any;

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            //if (this.lastMissionOperation != null && this.MissionOperation != null)
            //{
            //    this.lastMissionOperation.RequestedQuantity = this.InputQuantity.Value;
            //}

            //if (this.lastSelectedCompartmentDetail != null && this.AvailableQuantity.HasValue)
            //{
            //    this.lastSelectedCompartmentDetail.Stock = this.AvailableQuantity.Value;
            //}

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.CanInputAvailableQuantity = true;
            this.CanInputQuantity = true;
            this.CloseLine = true;
            this.RaisePropertyChanged(nameof(this.CanInputAvailableQuantity));
            this.RaisePropertyChanged(nameof(this.CanInputQuantity));

            this.Compartments = null;
            this.SelectedCompartment = null;

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            await base.OnAppearedAsync();

            //this.SetLastQuantity();
        }

        public override void OnMisionOperationRetrieved()
        {
            if (this.MissionOperation != null)
            {
                if (this.MissionOperation != null)
                {
                    this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
                }

                this.InputQuantity = this.MissionRequestedQuantity;
                //this.AvailableQuantity = this.MissionRequestedQuantity;

                this.RaisePropertyChanged(nameof(this.InputQuantity));
                this.RaisePropertyChanged(nameof(this.AvailableQuantity));
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            this.emptyOperationCommand.RaiseCanExecuteChanged();
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        private bool CanPartiallyCompleteOnEmptyCompartment()
        {
            this.CanConfirm =
                this.MissionOperation != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                this.InputQuantity.HasValue
                &&
                this.CanInputQuantity
                &&
                this.InputQuantity.Value == this.MissionRequestedQuantity;

            this.RaisePropertyChanged(nameof(this.CanConfirm));

            this.CanConfirmPartialOperation =
                this.MissionOperation != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                this.InputQuantity.HasValue
                &&
                this.CanInputQuantity
                &&
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value != this.MissionRequestedQuantity
                &&
                this.InputQuantity.Value <= this.AvailableQuantity;

            this.RaisePropertyChanged(nameof(this.CanConfirmPartialOperation));

            //return this.CanConfirm;
            return false;
        }

        private async Task PartiallyCompleteOnEmptyCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                if (!canComplete)
                {
                    this.ShowOperationCanceledMessage();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsOperationConfirmed = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        //private void SetLastQuantity()
        //{
        //    try
        //    {
        //        if (this.lastMissionOperation == null && this.MissionOperation != null)
        //        {
        //            this.lastMissionOperation = this.MissionOperation;
        //            this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //        }
        //        else if (this.MissionOperation != null)
        //        {
        //            if (this.lastMissionOperation.MissionId == this.MissionOperation.MissionId && this.lastMissionOperation.ItemCode == this.MissionOperation.ItemCode)
        //            {
        //                if (this.lastMissionOperation.RequestedQuantity != this.MissionRequestedQuantity)
        //                {
        //                    //this.MissionOperation.RequestedQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    //this.RaisePropertyChanged(nameof(this.MissionOperation));
        //                    this.InputQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    this.RaisePropertyChanged(nameof(this.InputQuantity));
        //                }
        //            }
        //            else
        //            {
        //                this.lastMissionOperation = this.MissionOperation;
        //                this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //            }
        //        }

        //        if (this.lastSelectedCompartmentDetail == null && this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //        }
        //        else if (this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            if (this.lastSelectedCompartmentDetail.ItemCode == this.SelectedCompartmentDetail.ItemCode)
        //            {
        //                if (this.lastMissionOperation.CompartmentId == this.MissionOperation.CompartmentId && this.lastMissionOperation.MissionId == this.MissionOperation.MissionId)
        //                {
        //                    if (this.lastSelectedCompartmentDetail.Stock != this.SelectedCompartmentDetail.Stock)
        //                    {
        //                        //this.SelectedCompartmentDetail.Stock = this.lastSelectedCompartmentDetail.Stock;
        //                        //this.RaisePropertyChanged(nameof(this.SelectedCompartmentDetail));
        //                        this.AvailableQuantity = this.lastSelectedCompartmentDetail.Stock;
        //                        this.RaisePropertyChanged(nameof(this.AvailableQuantity));
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //
        //    }
        //}

        #endregion
    }
}
