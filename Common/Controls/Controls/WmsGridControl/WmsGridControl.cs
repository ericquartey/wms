using System.Windows.Data;

namespace Ferretto.Common.Controls
{
  public class WmsGridControl : DevExpress.Xpf.Grid.GridControl
  {
    #region Methods

    protected override void OnInitialized(System.EventArgs e)
    {
      base.OnInitialized(e);

      this.DataContext = new WmsGridViewModel();

      var selectedItemBinding = new Binding("SelectedItem")
      {
        Mode = BindingMode.TwoWay,
        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
      };
      this.SetBinding(SelectedItemProperty, selectedItemBinding);
      this.SetBinding(ItemsSourceProperty, "Items");
    }

    #endregion Methods
  }
}
