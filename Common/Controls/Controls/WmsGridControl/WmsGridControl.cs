using System;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls
{
    public class WmsGridControl : DevExpress.Xpf.Grid.GridControl
    {
        #region Fields

        public static readonly DependencyProperty CurrentDataSourcesProperty = DependencyProperty.Register(
            nameof(CurrentDataSource),
            typeof(object),
            typeof(WmsGridControl),
            new PropertyMetadata(CurrentDataSourceChanged));

        #endregion Fields

        #region Properties

        public object CurrentDataSource
        {
            get => (object)this.GetValue(CurrentDataSourcesProperty);
            set => this.SetValue(CurrentDataSourcesProperty, value);
        }

        public Type ItemType { get; set; }

        #endregion Properties

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            if (this.ItemType == null)
            {
                throw new Exception("WmsGridControl ItemType is missing.");
            }
            base.OnInitialized(e);

            var viewModelClass = typeof(WmsGridViewModel<>);
            var constructedClass = viewModelClass.MakeGenericType(this.ItemType);
            this.DataContext = Activator.CreateInstance(constructedClass);

            var selectedItemBinding = new Binding("SelectedItem")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(SelectedItemProperty, selectedItemBinding);
            this.SetBinding(ItemsSourceProperty, "Items");
        }

        private static void CurrentDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is WmsGridControl gridControl)
            {
                if (gridControl.DataContext is IWmsGridViewModel dataContext)
                {
                    dataContext.SetDataSource(e.NewValue);
                }
            }
        }

        #endregion Methods
    }
}
