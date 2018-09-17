using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
    public class ItemDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataService dataService;
        private readonly IImageService imageService;
        private ICommand hideDetailsCommand;
        private ImageSource imgArticle;
        private Item item;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();

            this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
            this.dataService = ServiceLocator.Current.GetInstance<IDataService>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<AbcClass> AbcClassChoices => this.dataService.GetData<AbcClass>().AsEnumerable();

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
            (this.hideDetailsCommand = new DelegateCommand(ExecuteHideDetailsCommand));

        public ImageSource ImgArticle
        {
            get => this.imgArticle;
            set => this.SetProperty(ref this.imgArticle, value);
        }

        public Item Item
        {
            get => this.item;
            set => this.SetProperty(ref this.item, value);
        }

        public IEnumerable<ItemManagementType> ItemManagementTypeChoices => this.dataService.GetData<ItemManagementType>().AsEnumerable();

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public IEnumerable<MeasureUnit> UnitOfMeasurementChoices => this.dataService.GetData<MeasureUnit>().AsEnumerable();

        #endregion Properties

        #region Methods

        private static void ExecuteHideDetailsCommand()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ShowDetailsEventArgs<Item>(false));
        }

        private void ExecuteSaveCommand()
        {
            this.dataService.SaveChanges();

            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ItemChangedEvent<Item>(this.Item));
        }

        private void Initialize()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Subscribe<ItemSelectionChangedEvent<Item>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
        }

        private void OnItemSelectionChanged(Item selectedItem)
        {
            this.Item = selectedItem;

            if (selectedItem != null)
            {
                this.ImgArticle = selectedItem.Image != null ? this.imageService.GetImage(selectedItem.Image) : null;
            }
            else
            {
                this.ImgArticle = null;
            }
        }

        #endregion Methods
    }
}
