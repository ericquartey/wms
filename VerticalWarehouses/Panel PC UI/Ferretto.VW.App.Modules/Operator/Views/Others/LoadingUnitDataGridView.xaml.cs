using System.Windows.Controls;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class LoadingUnitDataGridView : UserControl
    {
        #region Constructors

        public LoadingUnitDataGridView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DataGrid.ScrollIntoView(this.DataGrid.SelectedItem);
        }

        #endregion
    }
}
