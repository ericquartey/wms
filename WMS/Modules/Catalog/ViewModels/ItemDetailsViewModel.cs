using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemDetailsViewModel : BaseNavigationViewModel
  {
    private readonly IImageService imageService;

    #region Properties
    private IItem item;
    public IItem Item
    {
      get => this.item;
      set => this.SetProperty(ref this.item, value);
    }

    private ImageSource imgArticle;
    public ImageSource ImgArticle
    {
      get => this.imgArticle;
      set => this.SetProperty(ref this.imgArticle, value);
    }
    #endregion

    #region Ctor
    public ItemDetailsViewModel(IImageService imageService)
    {
      this.Initialize();
      this.imageService = imageService;
    }
    #endregion

    #region Methods
    private void Initialize()
    {
      // Subscribe
      var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
      var navigationCompletedEvent = eventAggregator.GetEvent<ItemSelectionChangedEvent>();
      navigationCompletedEvent.Subscribe(item => this.OnItemSelectionChanged(item), ThreadOption.UIThread);
    }

    private void OnItemSelectionChanged(object selectedItemObj)
    {
      if (selectedItemObj is IItem selectedItem)
      {
        this.Item = selectedItem;
        this.ImgArticle = selectedItem.Image != null ? this.imageService.GetImage(selectedItem.Image) : null;
      }
      else
      {
        this.ImgArticle = null;
      }
    }
    #endregion

  }
}
