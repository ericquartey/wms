using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Ferretto.Common.BLL.Interfaces;
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
        typeof(IBusinessObject),
        typeof(WmsGridControl),
        new FrameworkPropertyMetadata(OnSelectedValueChanged));

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private string token;

        private IRefreshDataEntityViewModel wmsViewModel;

        #endregion Fields

        #region Properties

        public ICommand RefreshCommand
        {
            get => (ICommand)this.GetValue(RefreshCommandProperty);
            set => this.SetValue(RefreshCommandProperty, value);
        }

        public IBusinessObject SelectedBusinessObject
        {
            get => (IBusinessObject)this.GetValue(SelectedBusinessObjectProperty);
            set => this.SetValue(SelectedBusinessObjectProperty, value);
        }

        #endregion Properties

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
            if (d is WmsGridControl gridControl)
            {
                if (e.NewValue is IBusinessObject bo)
                {
                    SetSelectedItem(gridControl, bo);
                }
            }
        }

        private static void SetSelectedItem(WmsGridControl gridControl, IBusinessObject bo)
        {
            var rowHandle = gridControl.FindRowByValue(nameof(IBusinessObject.Id), bo.Id);
            gridControl.Dispatcher.BeginInvoke(new Action(() =>
                {
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
                }), DispatcherPriority.Loaded);
        }

        private void SetOperationsOnSelectedItem()
        {
            this.SelectedItemChanged += this.WmsGridControl_SelectedItemChanged;
        }

        private void SetToken()
        {
            this.Loaded += this.WmsGridControl_Loaded;            
        }

        private void WmsGridControl_Loaded(Object sender, RoutedEventArgs e)
        {
            var wmsViews = LayoutTreeHelper.GetVisualParents(this.Parent).OfType<WmsView>();
            if (wmsViews != null && wmsViews.Any())
            {
                var wmsView = wmsViews.First();
                this.token = wmsView.Token;
                var wmsViewViewModel = ((INavigableView)wmsView).DataContext as INavigableViewModel;
                this.wmsViewModel = wmsViewViewModel as IRefreshDataEntityViewModel;
                this.Loaded -= this.WmsGridControl_Loaded;
            }
        }

        private void WmsGridControl_SelectedItemChanged(Object sender, SelectedItemChangedEventArgs e)
        {
            this.SelectedBusinessObject = (e.NewItem is IBusinessObject bo) ? bo : null;
            if (this.SelectedBusinessObject != null)
            {
                this.eventService.Invoke(new ModelSelectionChangedEvent<IBusinessObject>(this.SelectedBusinessObject.Id, this.token));
            }
        }
        #endregion Methods
    }
}
