using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemDetailsViewModel : BindableBase, IItemDetailsViewModel
  {
    private readonly IImageService imageService;

    #region Properties
    private Item item;
    public Item Item
    {
      get { return item; }
      set
      {
        item = value;
        RaisePropertyChanged(nameof(Item));
      }
    }
    private ImageSource imgArticle;
    public ImageSource ImgArticle
    {
      get { return imgArticle; }
      set
      {
        imgArticle = value;
        RaisePropertyChanged(nameof(ImgArticle));
      }
    }
    #endregion

    #region Ctor
    public ItemDetailsViewModel(IImageService imageService)
    {
      Initialize();
      this.imageService = imageService;
    }
    #endregion

    #region Methods
    private void Initialize()
    {
      // Subscribe
      var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
      var navigationCompletedEvent = eventAggregator.GetEvent<ItemSelectionChangedEvent>();
      navigationCompletedEvent.Subscribe(item => OnItemSelectionChanged(item), ThreadOption.UIThread);
    }

    private void OnItemSelectionChanged(object selectedItemObj)
    {
      if (selectedItemObj is Item selectedItem && selectedItem.Image != null)
      {
        ImgArticle = imageService.GetImage(selectedItem.Image);
      }
      else
      {
        ImgArticle = null;
      }
    }
    #endregion

  }
}
