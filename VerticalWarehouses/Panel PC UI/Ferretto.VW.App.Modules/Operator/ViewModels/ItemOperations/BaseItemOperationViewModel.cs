using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseItemOperationViewModel : BaseMainViewModel
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

            await this.GetItemImageAsync();

            this.IsBackNavigationAllowed = true;
        }

        private async Task GetItemImageAsync()
        {
            try
            {
                this.Mission = await this.missionDataService.GetDetailsByIdAsync(this.MissionOperationsService.CurrentMission.Id);
                await this.LoadImageAsync(this.MissionOperationsService.CurrentMissionOperation.ItemCode);

                this.MissionOperation = this.MissionOperationsService.CurrentMissionOperation;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task LoadImageAsync(string code)
        {
            var stream = await this.WmsImagesProvider.GetImageAsync(code);
            this.ItemImage = Image.FromStream(stream);
        }

        #endregion
    }
}
