using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Prism.Commands;

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

        private Type itemType;

        private IRefreshDataEntityViewModel wmsViewModel;

        #endregion Fields

        #region Properties

        public Type ItemType
        {
            get => this.itemType;
            set
            {
                if (value != this.itemType)
                {
                    if (value?.GetInterface(typeof(IBusinessObject).FullName) != null)
                    {
                        this.itemType = value;
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"The value assigned to the {nameof(this.ItemType)} property must be of type {nameof(IBusinessObject)}", nameof(value));
                    }
                }
            }
        }

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

            this.DataContext = this.InstantiateViewModel();

            this.SetToken();

            this.SetupBindings();

            this.SetSelectedItem();
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

        private object InstantiateViewModel()
        {
            if (this.ItemType == null)
            {
                throw new InvalidOperationException("WmsGridControl ItemType is missing.");
            }

            var constructedClass = typeof(WmsGridViewModel<>).MakeGenericType(this.ItemType);
            return Activator.CreateInstance(constructedClass);
        }

        private void SetCmdRefreshBinding()
        {
            var myBinding = new Binding()
            {
                Source = this.DataContext,
                Path = new PropertyPath(nameof(Ferretto.Common.Controls.WmsGridViewModel<IBusinessObject>.CmdRefresh)),
                Mode = BindingMode.OneWayToSource,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, WmsGridControl.RefreshCommandProperty, myBinding);

            if (this.RefreshCommand == null)
            {
                this.RefreshCommand = new DelegateCommand(() => this.wmsViewModel.RefreshData());
            }
        }

        private void SetSelectedItem()
        {
            this.SelectedItemChanged += this.WmsGridControl_SelectedItemChanged;
        }

        private void SetToken()
        {
            this.Loaded += this.WmsGridControl_Loaded;
            this.Unloaded += this.WmsGridControl_Unloaded;
        }

        private void SetupBindings()
        {
            var selectedItemBinding = new Binding("SelectedItem")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(SelectedItemProperty, selectedItemBinding);
        }

        private void WmsGridControl_Loaded(Object sender, RoutedEventArgs e)
        {
            var wmsViews = LayoutTreeHelper.GetVisualParents(this.Parent).OfType<WmsView>();
            if (wmsViews != null && wmsViews.Any())
            {
                var wmsView = wmsViews.First();
                var wmsViewViewModel = ((INavigableView)wmsView).DataContext as INavigableViewModel;
                this.wmsViewModel = wmsViewViewModel as IRefreshDataEntityViewModel;
                var token = wmsViewViewModel.Token;
                var gridControlViewModel = (INavigableViewModel)this.DataContext;
                gridControlViewModel.Token = token;
                gridControlViewModel.Appear();
                this.SetCmdRefreshBinding();
            }
        }

        private void WmsGridControl_SelectedItemChanged(Object sender, SelectedItemChangedEventArgs e)
        {
            this.SelectedBusinessObject = (e.NewItem is IBusinessObject bo) ? bo : null;
        }

        private void WmsGridControl_Unloaded(Object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.WmsGridControl_Loaded;
            this.SelectedItemChanged -= this.WmsGridControl_SelectedItemChanged;
        }

        #endregion Methods
    }
}
