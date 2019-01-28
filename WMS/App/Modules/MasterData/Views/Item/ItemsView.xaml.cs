using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void FilterEditor_FilterChanged(object sender, DevExpress.Xpf.Core.FilteringUI.FilterChangedEventArgs e)
        {
            if (this.DataContext is ItemsViewModel viewModel)
            {
                viewModel.CustomFilter = e.Filter;
                e.Handled = true;
            }
        }

        #endregion Methods
    }
}
