using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Core.FilteringUI;

namespace Ferretto.Common.Controls
{
    public partial class WmsFilterControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FilteringContextProperty = DependencyProperty.Register(
                  nameof(FilteringContext), typeof(bool), typeof(WmsFilterControl));

        #endregion

        #region Constructors

        public WmsFilterControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += this.ItemsView_DataContextChanged;
        }

        #endregion

        #region Properties

        public bool FilteringContext
        {
            get => (bool)this.GetValue(FilteringContextProperty);
            set => this.SetValue(FilteringContextProperty, value);
        }

        #endregion

        #region Methods

        private void FilterEditor_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (this.DataContext is ICustomFilterViewModel customFilterContext)
            {
                customFilterContext.CustomFilter = e.Filter;

                e.Handled = true;
            }
        }

        private void ItemsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is INotifyPropertyChanged viewModel)
            {
                viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ICustomFilterViewModel.IsFilterEditorVisible))
            {
                this.FilterEditorContainer.Content = null;

                var filterControl = new FilterEditorControl();

                filterControl.SetBinding(
                    FilterEditorControl.ContextProperty,
                    new Binding("FilteringContext"));

                filterControl.FilterChanged += this.FilterEditor_FilterChanged;

                this.FilterEditorContainer.Content = filterControl;
            }
        }

        #endregion
    }
}
