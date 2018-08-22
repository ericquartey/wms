using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Xpf.Grid;

namespace Ferretto.Common.Controls
{
  public class WmsGridControl : GridControl
  {
    public WmsGridControl()
    {
      this.SetBinding(GridControl.ItemsSourceProperty, "Items");

      var selectedItemBinding = new Binding("SelectedItem")
      {
        Mode = BindingMode.TwoWay,
        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
      };
      this.SetBinding(GridControl.SelectedItemProperty, selectedItemBinding);
    }
  }
}
