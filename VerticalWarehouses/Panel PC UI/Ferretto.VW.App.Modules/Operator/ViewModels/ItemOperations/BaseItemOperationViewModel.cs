using System;
using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseItemOperationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMissionsDataService missionDataService;

        private System.Drawing.Image itemImage;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        #endregion

        #region Constructors

        public BaseItemOperationViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService)
            : base(PresentationMode.Operator)
        {
            this.WmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.missionDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public Image ItemImage
        {
            get => this.itemImage;
            set
            {
                var oldImage = this.itemImage;
                if (this.SetProperty(ref this.itemImage, value))
                {
                    oldImage?.Dispose();
                }
            }
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

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        protected IBayManager BayManager { get; }

        protected IMissionOperationsService MissionOperationsService { get; }

        protected IWmsImagesProvider WmsImagesProvider { get; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.ItemImage?.Dispose();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RetrieveMissionOperationAsync();

            this.IsBackNavigationAllowed = true;
        }

        public virtual void OnMisionOperationRetieved()
        {
        }

        protected async Task RetrieveMissionOperationAsync()
        {
            try
            {
                var missionOperation = this.MissionOperationsService.CurrentMissionOperation;

                if (missionOperation is null)
                {
                    this.NavigationService.GoBack();
                    return;
                }

                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory when !(this is ItemInventoryViewModel):
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.INVENTORY,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Pick when !(this is ItemPickViewModel):
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PICK,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Put when !(this is ItemPutViewModel):
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PUT,
                            null,
                            trackCurrentView: true);
                        break;
                }

                this.MissionOperation = missionOperation;

                this.Mission = await this.missionDataService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);

                await this.LoadImageAsync(this.MissionOperationsService.CurrentMissionOperation.ItemId.ToString());
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task LoadImageAsync(string imageKey)
        {
            var stream = await this.WmsImagesProvider.GetImageAsync(imageKey);
            this.ItemImage = Image.FromStream(stream);
        }

        #endregion
    }
}
