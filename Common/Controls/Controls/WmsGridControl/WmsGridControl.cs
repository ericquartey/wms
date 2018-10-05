using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls
{
    public class WmsGridControl : DevExpress.Xpf.Grid.GridControl
    {
        #region Fields

        public static readonly DependencyProperty CurrentDataSourceProperty = DependencyProperty.Register(
            nameof(CurrentDataSource),
            typeof(object),
            typeof(WmsGridControl),
            new PropertyMetadata(CurrentDataSourceChanged));

        private Type itemType;

        #endregion Fields

        #region Properties

        public object CurrentDataSource
        {
            get => this.GetValue(CurrentDataSourceProperty);
            set => this.SetValue(CurrentDataSourceProperty, value);
        }

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

        #endregion Properties

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            this.DisableColumnFiltering();

            this.DataContext = this.InstantiateViewModel();

            this.SetToken();

            this.SetupBindings();
        }

        private static void CurrentDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is WmsGridControl gridControl
                &&
                gridControl.DataContext is IWmsGridViewModel dataContext)
            {
                dataContext.SetDataSource(e.NewValue);
            }
        }

        private void DisableColumnFiltering()
        {
            this.View.AllowColumnFiltering = false;
        }

        private Object InstantiateViewModel()
        {
            if (this.ItemType == null)
            {
                throw new InvalidOperationException("WmsGridControl ItemType is missing.");
            }

            var viewModelClass = typeof(WmsGridViewModel<>);
            var constructedClass = viewModelClass.MakeGenericType(this.ItemType);

            return Activator.CreateInstance(constructedClass);
        }

        private void SetToken()
        {
            this.Loaded += this.WmsGridControl_Loaded;
        }

        private void SetupBindings()
        {
            var selectedItemBinding = new Binding("SelectedItem")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(SelectedItemProperty, selectedItemBinding);
            this.SetBinding(ItemsSourceProperty, "CurrentDataSource");
        }

        private void WmsGridControl_Loaded(Object sender, RoutedEventArgs e)
        {
            var wmsViews = LayoutTreeHelper.GetVisualParents(this.Parent).OfType<WmsView>();
            if (wmsViews != null && wmsViews.Any())
            {
                var wmsView = wmsViews.First();
                var wmsViewViewModel = ((INavigableView)wmsView).DataContext;
                var token = ((INavigableViewModel)wmsViewViewModel).Token;
                ((INavigableViewModel)this.DataContext).Token = token;
            }
        }

        #endregion Methods
    }
}
