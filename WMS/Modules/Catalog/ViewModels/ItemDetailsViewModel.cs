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
    #region Fields

    private readonly IImageService imageService;
    private IItem item;
    private ImageSource imgArticle;

    #endregion

    #region Properties

    public IItem Item
    {
      get => this.item;
      set => this.SetProperty(ref this.item, value);
    }

    public ImageSource ImgArticle
    {
      get => this.imgArticle;
      set => this.SetProperty(ref this.imgArticle, value);
    }

    #endregion

    #region Constructors

    public ItemDetailsViewModel(IImageService imageService)
    {
      this.Initialize();
      this.imageService = imageService;
    }

    #endregion

    #region Methods

    private void Initialize()
    {
      ServiceLocator.Current.GetInstance<IEventService>()
        .Subscribe<ItemSelectionChangedEvent<IItem>>(eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
    }

    private void OnItemSelectionChanged(IItem selectedItem)
    {
      if (selectedItem != null)
      {
        this.Item = selectedItem;
        this.ImgArticle = selectedItem.Image != null ? this.imageService.GetImage(selectedItem.Image) : null;
      }
      else
      {
        this.ImgArticle = null;
      }
    }

    private void SaveItem()
    {
      // TODO: call data saving service

      ServiceLocator.Current.GetInstance<IEventService>()
        .Invoke(new ItemChangedEvent<IItem>(this.Item));
    }

    #endregion

  }
}
