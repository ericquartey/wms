using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.Devices.LaserPointer;
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

        public override Task OnAppearedAsync()
        {
            this.CanInputAvailableQuantity = true;
            this.CanInputQuantity = true;
            this.RaisePropertyChanged(nameof(this.CanInputAvailableQuantity));
            this.RaisePropertyChanged(nameof(this.CanInputQuantity));

            this.Compartments = null;
            this.SelectedCompartment = null;

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            return base.OnAppearedAsync();
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
                this.AvailableQuantity = this.MissionRequestedQuantity;
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
                this.IsInputQuantityValid
                &&
                this.InputQuantity.Value > 0;

            this.RaisePropertyChanged(nameof(this.CanConfirm));

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

        #endregion
    }
}
