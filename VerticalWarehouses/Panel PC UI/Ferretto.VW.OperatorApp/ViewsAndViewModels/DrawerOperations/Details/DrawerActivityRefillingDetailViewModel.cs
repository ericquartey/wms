﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Prism.Events;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details
{
    public class DrawerActivityRefillingDetailViewModel : BaseViewModel, IDrawerActivityRefillingDetailViewModel
    {
        #region Fields

        private readonly IWmsImagesProvider wmsImagesProvider;

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

        public DrawerActivityRefillingDetailViewModel(IWmsImagesProvider wmsImagesProvider)
        {
            if (wmsImagesProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsImagesProvider));
            }

            this.wmsImagesProvider = wmsImagesProvider;
        }

        #endregion

        #region Properties

        public string Batch { get => this.batch; set => this.SetProperty(ref this.batch, value); }

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

        public override async Task OnEnterViewAsync()
        {
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
            var imageStream = await this.wmsImagesProvider.GetImageAsync(this.ItemDetail.Image);
            this.Image = Image.FromStream(imageStream);
        }

        #endregion
    }
}
