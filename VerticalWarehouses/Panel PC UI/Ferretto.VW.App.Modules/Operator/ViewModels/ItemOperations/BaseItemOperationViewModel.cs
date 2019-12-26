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

        private readonly IMissionsWmsWebService missionWmsWebService;

        private bool canInputQuantity;

        private bool isWaitingForResponse;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        #endregion

        #region Constructors

        public BaseItemOperationViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsWmsWebService missionsWmsWebService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.WmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.missionWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public string ItemId => this.MissionOperationsService.CurrentMissionOperation?.ItemId.ToString();

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

        public virtual void RaiseCanExecuteChanged()
        {
            // do nothing
        }

        protected async Task RetrieveMissionOperationAsync()
        {
            var newMissionOperation = this.MissionOperationsService.CurrentMissionOperation;

            if (newMissionOperation is null || (this.MissionOperation != null && this.MissionOperation.Type != newMissionOperation.Type))
            {
                this.NavigationService.GoBack();
                return;
            }

            try
            {
                this.MissionOperation = newMissionOperation;

                this.Mission = await this.missionWmsWebService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);

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
