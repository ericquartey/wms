using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IItemsWmsWebService itemsWmsWebService;

        private readonly IMissionsWmsWebService missionWmsWebService;

        private bool canInputQuantity;

        private string measureUnit;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        private double quantityIncrement;

        private int? quantityTolerance;

        #endregion

        #region Constructors

        public BaseItemOperationViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsWmsWebService missionsWmsWebService,
            IItemsWmsWebService itemsWmsWebService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.WmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.missionWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
            this.itemsWmsWebService = itemsWmsWebService ?? throw new ArgumentNullException(nameof(itemsWmsWebService));
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

        public string ItemId => this.MissionOperationsService.CurrentMissionOperation?.ItemId.ToString();

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
            set => this.SetProperty(ref this.missionOperation, value);
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

        protected IWmsImagesProvider WmsImagesProvider { get; }

        #endregion

        #region Methods

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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            // do nothing
        }

        protected async Task RetrieveMissionOperationAsync()
        {
            try
            {
                if (this.MissionOperationsService.CurrentMissionOperation is null)
                {
                    this.NavigationService.GoBack();
                    return;
                }

                this.MissionOperation = this.MissionOperationsService.CurrentMissionOperation;

                this.Mission = await this.missionWmsWebService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);

                var item = await this.itemsWmsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                this.QuantityTolerance = item.PickTolerance ?? 0;
                this.MeasureUnit = item.MeasureUnitDescription;

                this.RaisePropertyChanged(nameof(this.ItemId));

                this.OnMisionOperationRetrieved();
            }
            catch (Exception ex)
            {
                this.NavigationService.GoBack();
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
