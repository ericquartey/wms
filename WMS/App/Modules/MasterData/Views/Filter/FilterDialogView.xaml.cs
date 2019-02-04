using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class FilterDialogView : WmsDialogView
    {
        #region Constructors

        public FilterDialogView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void FilterEditor_FilterChanged(object sender, DevExpress.Xpf.Core.FilteringUI.FilterChangedEventArgs e)
        {
            if (this.DataContext is FilterDialogViewModel viewModel)
            {
                viewModel.Filter = e.Filter;
                e.Handled = true;
            }
        }

        #endregion
    }
}
