using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
  public class WmsGridViewModel<TModel, TId> : BindableBase, IWmsGridViewModel where TModel : IModel<TId>
  {
    #region Fields

    private readonly IEntityService<TModel, TId> entityService;
    private IEnumerable<TModel> items;
    private TModel selectedItem;

    #endregion

    #region Properties

    public IEnumerable<TModel> Items => this.items;

    public TModel SelectedItem
    {
      get => this.selectedItem;
      set
      {
        if (this.SetProperty(ref this.selectedItem, value))
        {
          this.NotifySelectionChanged();
        }
      }
    }

    #endregion

    #region Constructors

    public WmsGridViewModel()
    {
      this.entityService = ServiceLocator.Current.GetInstance<IEntityService<TModel, TId>>();

      this.Initialize();
    }

    #endregion

    #region Methods

    private void Initialize()
    {
      this.RefreshGrid();
    }

    public void RefreshGrid()
    {
      this.items = this.entityService.GetAll();
    }

    protected void NotifySelectionChanged()
    {
      ServiceLocator.Current.GetInstance<IEventService>()
        .Invoke(new ItemSelectionChangedEvent<TModel>(this.selectedItem));
    }

    #endregion
  }
}
