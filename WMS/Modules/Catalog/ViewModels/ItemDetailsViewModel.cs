using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemDetailsViewModel : BaseNavigationViewModel
  {
    #region Fields

    private readonly IImageService imageService;

    private ImageSource imgArticle;
    private IItem item;

    #endregion Fields

    #region Constructors

    public ItemDetailsViewModel()
    {
      this.Initialize();
      this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
    }

    #endregion Constructors

    #region Properties

    public ImageSource ImgArticle
    {
      get => this.imgArticle;
      set => this.SetProperty(ref this.imgArticle, value);
    }

    public IItem Item
    {
      get => this.item;
      set => this.SetProperty(ref this.item, value);
    }

    #endregion Properties

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

    #endregion Methods
  }
}
