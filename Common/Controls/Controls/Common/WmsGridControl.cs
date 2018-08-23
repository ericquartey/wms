using System.Windows.Data;
using Ferretto.Common.BLL.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
  public class WmsGridControl<TModel, TId> : DevExpress.Xpf.Grid.GridControl where TModel : IModel<TId>
  {
    protected WmsGridControl()
    {
    }

    protected override void OnInitialized(System.EventArgs e)
    {
      base.OnInitialized(e);

      this.DataContext = ServiceLocator.Current.GetInstance<WmsGridViewModel<TModel, TId>>() ?? new WmsGridViewModel<TModel, TId>();

      var selectedItemBinding = new Binding("SelectedItem")
      {
        Mode = BindingMode.TwoWay,
        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
      };
      this.SetBinding(SelectedItemProperty, selectedItemBinding);
      this.SetBinding(ItemsSourceProperty, "Items");
    }
  }

}
