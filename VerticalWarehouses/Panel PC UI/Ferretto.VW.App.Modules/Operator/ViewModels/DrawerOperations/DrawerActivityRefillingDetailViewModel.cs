using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerActivityRefillingDetailViewModel : BaseDrawerOperationViewModel
    {
        #region Fields

        private string batch;

        private Image image;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private string listRow;

        private string materialStatus;

        private string packagingType;

        private string position;

        private string productionDate;

        private string requestedQuantity;

        #endregion

        #region Constructors

        public DrawerActivityRefillingDetailViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public string Batch { get => this.batch; set => this.SetProperty(ref this.batch, value); }

        public override EnableMask EnableMask => EnableMask.None;

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public string ItemCode { get => this.itemCode; set => this.SetProperty(ref this.itemCode, value); }

        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public DrawerActivityItemDetail ItemDetail { get; set; }

        public string ListCode { get => this.listCode; set => this.SetProperty(ref this.listCode, value); }

        public string ListDescription { get => this.listDescription; set => this.SetProperty(ref this.listDescription, value); }

        public string ListRow { get => this.listRow; set => this.SetProperty(ref this.listRow, value); }

        public string MaterialStatus { get => this.materialStatus; set => this.SetProperty(ref this.materialStatus, value); }

        public string PackagingType { get => this.packagingType; set => this.SetProperty(ref this.packagingType, value); }

        public string Position { get => this.position; set => this.SetProperty(ref this.position, value); }

        public string ProductionDate { get => this.productionDate; set => this.SetProperty(ref this.productionDate, value); }

        public string RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Batch = this.ItemDetail.Batch;
            this.ItemCode = this.ItemDetail.ItemCode;
            this.ItemDescription = this.ItemDetail.ItemDescription;
            this.ListCode = this.ItemDetail.ListCode;
            this.ListDescription = this.ItemDetail.ListDescription;
            this.ListRow = this.ItemDetail.ListRow;
            this.MaterialStatus = this.ItemDetail.MaterialStatus;
            this.PackagingType = this.ItemDetail.PackageType;
            this.Position = this.ItemDetail.Position;
            this.ProductionDate = this.ItemDetail.ProductionDate;
            this.RequestedQuantity = this.ItemDetail.RequestedQuantity;
            try
            {
                var imageStream = await this.WmsImagesProvider.GetImageAsync(this.ItemDetail.Image);
                this.Image = Image.FromStream(imageStream);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
