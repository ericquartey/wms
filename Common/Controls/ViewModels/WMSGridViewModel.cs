using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls.ViewModels
{
  public class WmsGridViewModel<TModel, TId> : BindableBase where TModel : IModel<TId>
  {
    private readonly IEntityService<TModel, TId> entityService;
    private TModel selectedItem;

    protected WmsGridViewModel(IEntityService<TModel, TId> entityService)
    {
      this.entityService = entityService;

      this.Initialize();
    }

    private void Initialize()
    {
      this.entityService.GetAll();
    }

    public IEnumerable<TModel> Items
    {
      get { return this.entityService.GetAll(); }
    }

    public TModel SelectedItem
    {
      get { return this.selectedItem; }
      set { this.SetProperty(ref this.selectedItem, value); }
    } 
  }
}
