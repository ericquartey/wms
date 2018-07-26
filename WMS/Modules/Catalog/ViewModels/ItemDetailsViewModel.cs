using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ferretto.WMS.Comp.Catalog
{
  public class ItemDetailsViewModel : BindableBase, IItemDetailsViewModel
  {

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
    public ItemDetailsViewModel()
    {
      this.Initialize();
    }
    #endregion

    #region Methods
    private void Initialize()
    {
      // Subscribe
      var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
      var navigationCompletedEvent = eventAggregator.GetEvent<ItemSelectionChangedEvent>();
      navigationCompletedEvent.Subscribe(OnItemSelectionChanged, ThreadOption.UIThread);
    }

    private void OnItemSelectionChanged(object selectedItemObj)
    {
      this.Item = selectedItemObj as Item;
      if (this.item != null)
      {
        // TBD later       
        this.ImgArticle = new BitmapImage(new Uri("pack://application:,,,/Ferretto.WMS.Themes;component/Images/Articolo1.jpg"));
      }
    }
    #endregion

  }
}
