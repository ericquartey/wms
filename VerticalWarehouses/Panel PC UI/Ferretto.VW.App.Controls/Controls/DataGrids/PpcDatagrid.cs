using System.Windows.Controls;

namespace Ferretto.VW.App.Controls
{
    public class PpcDataGrid : DataGrid
    {
        #region Methods

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (!(this.SelectedItem is null))
            {
                this.UpdateLayout();
                if (this.SelectedItem != null)
                {
                    this.ScrollIntoView(this.SelectedItem);
                }
            }
        }

        #endregion
    }
}
