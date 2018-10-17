using System;
using System.Linq;
using System.Reflection;
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

        private Type itemType;

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

        #endregion Properties

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            this.DisableColumnFiltering();

            this.UpdateFilterTiles();

            this.DataContext = this.InstantiateViewModel();

            this.SetToken();

            this.SetupBindings();
        }

        private async void AsyncOperationCompletedAsync(Object sender, RoutedEventArgs e)
        {
            var wmsView = (LayoutTreeHelper.GetVisualParents(this)
                    .OfType<INavigableView>()
                    .FirstOrDefault());

            if (wmsView?.DataContext is IEntityListViewModel viewModel)
            {
                await viewModel.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
                this.AsyncOperationCompleted -= this.AsyncOperationCompletedAsync;
            }
        }

        private void DisableColumnFiltering()
        {
            this.View.AllowColumnFiltering = false;
        }

        private object InstantiateViewModel()
        {
            if (this.ItemType == null)
            {
                throw new InvalidOperationException("WmsGridControl ItemType is missing.");
            }

            var viewModelClass = typeof(WmsGridViewModel<,>);
            var idType = ((TypeInfo)this.itemType.GetInterface(typeof(IBusinessObject).FullName)).DeclaredProperties.First();
            var constructedClass = viewModelClass.MakeGenericType(this.ItemType, idType.PropertyType);
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
        }

        private void UpdateFilterTiles()
        {
            this.AsyncOperationCompleted += this.AsyncOperationCompletedAsync;
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
