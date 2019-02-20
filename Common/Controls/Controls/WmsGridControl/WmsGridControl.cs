using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsGridControl : GridControl
    {
        #region Fields

        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(
            nameof(RefreshCommand),
            typeof(ICommand),
            typeof(WmsGridControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedBusinessObjectProperty = DependencyProperty.Register(
        nameof(SelectedBusinessObject),
        typeof(IModel<int>),
        typeof(WmsGridControl),
        new FrameworkPropertyMetadata(OnSelectedValueChanged));

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private string token;

        private IRefreshDataEntityViewModel wmsViewModel;

        #endregion

        #region Properties

        public ICommand RefreshCommand
        {
            get => (ICommand)this.GetValue(RefreshCommandProperty);
            set => this.SetValue(RefreshCommandProperty, value);
        }

        public IModel<int> SelectedBusinessObject
        {
            get => (IModel<int>)this.GetValue(SelectedBusinessObjectProperty);
            set => this.SetValue(SelectedBusinessObjectProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            this.SetValue(ScrollBarExtensions.ScrollBarModeProperty, ScrollBarMode.TouchOverlap);

            this.SetToken();

            this.SetOperationsOnSelectedItem();
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsGridControl gridControl && e.NewValue is IModel<int> bo)
            {
                SetSelectedItem(gridControl, bo);
            }
        }

        private static void SetSelectedItem(WmsGridControl gridControl, IModel<int> bo)
        {
            var rowHandle = gridControl.FindRowByValue(nameof(IModel<int>.Id), bo.Id);
            if (rowHandle > -1)
            {
                gridControl.View.FocusedRowHandle = rowHandle;
                gridControl.SelectItem(rowHandle);
                gridControl.SelectedItem = gridControl.CurrentItem;
            }
            else
            {
                gridControl.SelectedItem = gridControl.CurrentItem = null;
            }

            gridControl.SelectedItem = gridControl.CurrentItem;
        }

        private void SetOperationsOnSelectedItem()
        {
            this.SelectedItemChanged += this.WmsGridControl_SelectedItemChanged;
        }

        private void SetToken()
        {
            this.Loaded += this.WmsGridControl_Loaded;
        }

        private void WmsGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            var wmsViews = LayoutTreeHelper.GetVisualParents(this.Parent).OfType<WmsView>();
            if (wmsViews != null && wmsViews.Any())
            {
                var wmsView = wmsViews.First();
                this.token = wmsView.Token;
                var wmsViewViewModel = ((INavigableView)wmsView).DataContext as INavigableViewModel;
                this.wmsViewModel = wmsViewViewModel as IRefreshDataEntityViewModel;
                this.SelectedItem = -1;
                this.Loaded -= this.WmsGridControl_Loaded;
            }
        }

        private void WmsGridControl_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            this.SelectedBusinessObject = e.NewItem is IModel<int> bo ? bo : null;
            if (this.SelectedBusinessObject != null)
            {
                this.eventService.Invoke(new ModelSelectionChangedPubSubEvent<IModel<int>>(this.SelectedBusinessObject.Id, this.token));
            }
        }

        #endregion
    }
}
