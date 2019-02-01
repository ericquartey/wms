using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public partial class ItemsView : WmsView
    {
        #region Constructors

        public ItemsView()
        {
            this.InitializeComponent();

            this.DataContextChanged += this.ItemsView_DataContextChanged;
        }

        #endregion Constructors

        #region Methods

        private void ItemsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EntityPagedListViewModel<Item> viewModel)
            {
                // pass the grid's filtering context to the view model
                // so that view model's commands can use it
                viewModel.FilteringContext = this.MainGridControl.FilteringContext;
            }
        }

        #endregion Methods
    }
}
