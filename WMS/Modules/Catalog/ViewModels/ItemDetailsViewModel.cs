using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
  public class ItemDetailsViewModel : BaseNavigationViewModel
  {
    #region Fields

    private readonly IImageService imageService;

    private ICommand hideDetailsCommand;
    private ImageSource imgArticle;
    private Common.DAL.Models.Item item;

    #endregion Fields

    #region Constructors

    public ItemDetailsViewModel()
    {
      this.Initialize();
      this.imageService = ServiceLocator.Current.GetInstance<IImageService>();
    }

    #endregion Constructors

    #region Properties

    public ICommand HideDetailsCommand => this.hideDetailsCommand ?? (this.hideDetailsCommand = new DelegateCommand(ExecuteHideDetailsCommand));

    public ImageSource ImgArticle
    {
      get => this.imgArticle;
      set => this.SetProperty(ref this.imgArticle, value);
    }

    public Common.DAL.Models.Item Item
    {
      get => this.item;
      set => this.SetProperty(ref this.item, value);
    }

    #endregion Properties

    #region Methods

    private static void ExecuteHideDetailsCommand()
    {
      ServiceLocator.Current.GetInstance<IEventService>().Invoke(new ShowDetailsEventArgs<Common.DAL.Models.Item>(false));
    }

    private void Initialize()
    {
      ServiceLocator.Current.GetInstance<IEventService>()
        .Subscribe<ItemSelectionChangedEvent<Common.DAL.Models.Item>>(eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
    }

    private void OnItemSelectionChanged(Common.DAL.Models.Item selectedItem)
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
        .Invoke(new ItemChangedEvent<Common.DAL.Models.Item>(this.Item));
    }

    #endregion Methods
  }
}
