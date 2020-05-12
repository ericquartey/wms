using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private bool canInputQuantity;

        private string measureUnit;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        private double quantityIncrement;

        private int? quantityTolerance;

        #endregion

        #region Constructors

        public BaseItemOperationViewModel(
            IMachineItemsWebService itemsWebService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            protected set => this.SetProperty(ref this.canInputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public string ItemId => this.missionOperation?.ItemId.ToString();

        public string MeasureUnit
        {
            get => this.measureUnit;
            set => this.SetProperty(ref this.measureUnit, value);
        }

        public MissionWithLoadingUnitDetails Mission
        {
            get => this.mission;
            private set => this.SetProperty(ref this.mission, value);
        }

        public MissionOperation MissionOperation
        {
            get => this.missionOperation;
            private set => this.SetProperty(ref this.missionOperation, value);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
                }
            }
        }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        protected IBayManager BayManager { get; }

        protected IDialogService DialogService { get; private set; }

        protected IMissionOperationsService MissionOperationsService { get; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.MissionOperation = null;
            this.Mission = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RetrieveMissionOperationAsync();

            this.CanInputQuantity = true;

            this.IsBackNavigationAllowed = true;
        }

        public virtual void OnMisionOperationRetrieved()
        {
            // do nothing
        }

        protected async Task RetrieveMissionOperationAsync()
        {
            if (this.MissionOperationsService.ActiveWmsOperation is null)
            {
                // ?????????????? this.NavigationService.GoBack();
                return;
            }

            this.Mission = this.MissionOperationsService.ActiveWmsMission;
            this.MissionOperation = this.MissionOperationsService.ActiveWmsOperation;

            if (this.missionOperation.Type == MissionOperationType.LoadingUnitCheck)
            {
                return;
            }

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                this.QuantityTolerance = item.PickTolerance ?? 0;
                this.MeasureUnit = item.MeasureUnitDescription;

                this.RaisePropertyChanged(nameof(this.ItemId));
                this.RaisePropertyChanged(nameof(this.MeasureUnit));
                this.RaisePropertyChanged(nameof(this.QuantityTolerance));

                this.OnMisionOperationRetrieved();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.NavigationService.GoBack();
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
