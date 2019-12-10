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

        public virtual void OnMisionOperationRetrieved()
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

                this.Mission = await this.missionDataService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);

                await this.LoadImageAsync(this.MissionOperationsService.CurrentMissionOperation.ItemId.ToString());

                this.OnMisionOperationRetrieved();
            }
            catch (Exception ex)
            {
                this.NavigationService.GoBack();
                this.ShowNotification(ex);
            }
        }

        private async Task LoadImageAsync(string imageKey)
        {
            try
            {
                var stream = await this.WmsImagesProvider.GetImageAsync(imageKey);
                this.ItemImage = Image.FromStream(stream);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
