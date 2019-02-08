using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public partial class WmsFilterControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FilteringContextProperty = DependencyProperty.Register(
                  nameof(FilteringContext), typeof(FilteringUIContext), typeof(WmsFilterControl));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
                 nameof(Filter), typeof(CriteriaOperator), typeof(WmsFilterControl));

        private ICommand clearFilterCommand;

        #endregion

        #region Constructors

        public WmsFilterControl()
        {
            this.InitializeComponent();

            this.FilterStackPanel.DataContext = this;
        }

        #endregion

        #region Properties

        public ICommand ClearFilterCommand => this.clearFilterCommand ??
           (this.clearFilterCommand = new DelegateCommand(
               this.ExecuteClearFilterCommand));

        public CriteriaOperator Filter
        {
            get => (CriteriaOperator)this.GetValue(FilterProperty);
            set => this.SetValue(FilterProperty, value);
        }

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

            this.Filter = null;
            this.FilterEditorContainer.Content = filterControl;
        }

        private void FilterEditor_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            this.Filter = e.Filter;
            e.Handled = true;
        }

        #endregion
    }
}
