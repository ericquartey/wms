using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Xpf.Core.FilteringUI;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public partial class WmsFilterControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FilteringContextProperty = DependencyProperty.Register(
                  nameof(FilteringContext), typeof(FilteringUIContext), typeof(WmsFilterControl));

        private ICommand clearFilterCommand;

        #endregion

        #region Constructors

        public WmsFilterControl()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        #endregion

        #region Properties

        public ICommand ClearFilterCommand => this.clearFilterCommand ??
           (this.clearFilterCommand = new DelegateCommand(
               this.ExecuteClearFilterCommand));

        public FilteringUIContext FilteringContext
        {
            get => (FilteringUIContext)this.GetValue(FilteringContextProperty);
            set => this.SetValue(FilteringContextProperty, value);
        }

        #endregion

        #region Methods

        private void ExecuteClearFilterCommand()
        {
            if (this.FilterEditorContainer.Content is FilterEditorControl filterEditorControl)
            {
                filterEditorControl.FilterChanged -= this.FilterEditor_FilterChanged;
            }

            var filterControl = new FilterEditorControl();

            filterControl.SetBinding(
                FilterEditorControl.ContextProperty,
                new Binding("FilteringContext"));

            filterControl.FilterChanged += this.FilterEditor_FilterChanged;

            this.FilterEditorContainer.Content = filterControl;
        }

        private void FilterEditor_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (this.DataContext is ICustomFilterViewModel customFilterContext)
            {
                customFilterContext.CustomFilter = e.Filter;

                e.Handled = true;
            }
        }

        #endregion
    }
}
