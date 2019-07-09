using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details
{
    public class DrawerActivityPickingDetailViewModel : BindableBase, IDrawerActivityPickingDetailViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string batch;

        private IUnityContainer container;

        private Image image;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private string listRow;

        private string materialStatus;

        private string packagingType;

        private string packingListCode;

        private string packingListDescription;

        private string position;

        private string productionDate;

        private string requestedQuantity;

        private IWmsImagesProvider wmsImagesProvider;

        #endregion

        #region Constructors

        public DrawerActivityPickingDetailViewModel(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
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

        public BindableBase NavigationViewModel { get; set; }

        public string PackagingType { get => this.packagingType; set => this.SetProperty(ref this.packagingType, value); }

        public string PackingListCode { get => this.packingListCode; set => this.SetProperty(ref this.packingListCode, value); }

        public string PackingListDescription { get => this.packingListDescription; set => this.SetProperty(ref this.packingListDescription, value); }

        public string Position { get => this.position; set => this.SetProperty(ref this.position, value); }

        public string ProductionDate { get => this.productionDate; set => this.SetProperty(ref this.productionDate, value); }

        public string RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.wmsImagesProvider = this.container.Resolve<IWmsImagesProvider>();
        }

        public async Task OnEnterViewAsync()
        {
            this.Batch = this.ItemDetail.Batch;
            this.ItemCode = this.ItemDetail.ItemCode;
            this.ItemDescription = this.ItemDetail.ItemDescription;
            this.ListCode = this.ItemDetail.ListCode;
            this.ListDescription = this.ItemDetail.ListDescription;
            this.ListRow = this.ItemDetail.ListRow;
            this.MaterialStatus = this.ItemDetail.MaterialStatus;
            this.PackagingType = this.ItemDetail.PackageType;
            this.PackingListCode = this.ItemDetail.PackingListCode;
            this.PackingListDescription = this.ItemDetail.PackingListDescription;
            this.Position = this.ItemDetail.Position;
            this.ProductionDate = this.ItemDetail.ProductionDate;
            this.RequestedQuantity = this.ItemDetail.RequestedQuantity;
            var imageStream = await this.wmsImagesProvider.GetImageAsync(this.ItemDetail.Image);
            this.Image = Image.FromStream(imageStream);
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
