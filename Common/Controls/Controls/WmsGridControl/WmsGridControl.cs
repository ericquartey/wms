using System;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.Common.Controls
{
    public class WmsGridControl : DevExpress.Xpf.Grid.GridControl
    {
        #region Properties

        public Type ItemType { get; set; }

        #endregion

        #region Dependency properties  

        public object CurrentDataSource
        {
            get => (object)this.GetValue(CurrentDataSourcesProperty);
            set => this.SetValue(CurrentDataSourcesProperty, value);
        }

        public static readonly DependencyProperty CurrentDataSourcesProperty = DependencyProperty.Register(
            nameof(CurrentDataSource),
            typeof(object),
            typeof(WmsGridControl),
            new PropertyMetadata(CurrentFilterChanged));

        private static void CurrentFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is WmsGridControl gridControl)
            {                
                if (gridControl.DataContext is IWmsGridViewModel dataContext)
                {
                    dataContext.SetDataSource(e.NewValue);
                }
            }
        }

        #endregion

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            if (this.ItemType == null)
            {
                throw new Exception("WmsGridControl ItemType is missing.");
            }
            base.OnInitialized(e);

            Type viewModelClass = typeof(WmsGridViewModel<>);
            Type constructedClass = viewModelClass.MakeGenericType(this.ItemType);
            this.DataContext = Activator.CreateInstance(constructedClass);            

            var selectedItemBinding = new Binding("SelectedItem")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            this.SetBinding(SelectedItemProperty, selectedItemBinding);
            this.SetBinding(ItemsSourceProperty, "Items");
        }

        #endregion Methods
    }
}
