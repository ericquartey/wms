using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls.ViewModels
{
  public class WmsGridViewModel<TModel, TId> : BindableBase where TModel : IModel<TId>
  {
    private readonly IEntityService<TModel, TId> entityService;

    protected WmsGridViewModel(IEntityService<TModel, TId> entityService)
    {
      this.entityService = entityService;

      this.Initialize();
    }

    private void Initialize()
    {
      this.items = this.entityService.GetAll();
    }

    private IEnumerable<TModel> items;
    public IEnumerable<TModel> Items => this.items;

    private TModel selectedItem;
    public TModel SelectedItem
    {
      get => this.selectedItem;
      set => this.SetProperty(ref this.selectedItem, value);
    }
  }
}
