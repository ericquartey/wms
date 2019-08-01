using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using DevExpress.Xpf.Editors.Filtering;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public partial class WmsFilterControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FilteringContextProperty = DependencyProperty.Register(
                  nameof(FilteringContext), typeof(IFilteredComponent), typeof(WmsFilterControl));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
                 nameof(Filter), typeof(CriteriaOperator), typeof(WmsFilterControl));

        private ICommand clearFilterCommand;

        private ICommand filterCommand;

        #endregion

        #region Constructors

        public WmsFilterControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public ICommand ClearFilterCommand => this.clearFilterCommand ??
           (this.clearFilterCommand = new DelegateCommand(
               this.ExecuteClearFilterCommand));

        public ICommand FilterCommand => this.filterCommand ??
           (this.filterCommand = new DelegateCommand(
               this.ExecuteFilterCommand));

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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.FilterStackPanel.DataContext = this;
        }

        private void ExecuteClearFilterCommand()
        {
            this.filterEditor.FilterCriteria = null;
        }

        private void ExecuteFilterCommand()
        {
            this.FilterEditorContainer.Focus();
            this.filterEditor.ApplyFilter();
        }

        #endregion
    }
}
