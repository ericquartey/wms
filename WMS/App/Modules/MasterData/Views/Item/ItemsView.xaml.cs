using System.Windows.Data;
using DevExpress.Xpf.Core.FilteringUI;
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

        #endregion

        #region Methods

        private void FilterEditor_FilterChanged(object sender, DevExpress.Xpf.Core.FilteringUI.FilterChangedEventArgs e)
        {
            if (this.DataContext is EntityPagedListViewModel<Item> viewModel)
            {
                viewModel.CustomFilter = e.Filter;

                e.Handled = true;
            }
        }

        private void ItemsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is EntityPagedListViewModel<Item> viewModel)
            {
                viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is EntityPagedListViewModel<Item> viewModel
                &&
                e.PropertyName == nameof(EntityPagedListViewModel<Item>.IsFilterEditorVisible))
            {
                this.FilterEditorContainer.Content = null;

                var filterControl = FilterEditorControl();

                var binding = new Binding("FilteringContext")
                {
                    ElementName = "MainGridControl"
                };

                filterControl.SetBinding(FilterEditorControl.ContextProperty, binding);

                this.FilterEditorContainer.Content = filterControl;
            }
        }

        #endregion
    }
}
