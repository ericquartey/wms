using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
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
            nameof(RefreshCommandProperty),
            typeof(ICommand),
            typeof(WmsGridControl),
            new FrameworkPropertyMetadata(null));

        private Type itemType;
        private IEntityListViewModel wmsViewModel;

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

        private async void AsyncOperationCompletedAsync(Object sender, RoutedEventArgs e)
        {
            var wmsView = (LayoutTreeHelper.GetVisualParents(this)
                    .OfType<INavigableView>()
                    .FirstOrDefault());

            if (wmsView?.DataContext is IEntityListViewModel viewModel)
            {
                this.wmsViewModel = viewModel;
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

            var constructedClass = typeof(WmsGridViewModel<>).MakeGenericType(this.ItemType);
            return Activator.CreateInstance(constructedClass);
        }

        private void SetCmdRefreshBinding()
        {
            Binding myBinding = new Binding()
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

        private void WmsGridControl_Loaded(Object sender, RoutedEventArgs e)
        {
            var wmsViews = LayoutTreeHelper.GetVisualParents(this.Parent).OfType<WmsView>();
            if (wmsViews != null && wmsViews.Any())
            {
                var wmsView = wmsViews.First();
                var wmsViewViewModel = ((INavigableView)wmsView).DataContext as INavigableViewModel;
                var token = wmsViewViewModel.Token;
                var gridControlViewModel = (INavigableViewModel)this.DataContext;
                gridControlViewModel.Token = token;
                gridControlViewModel.Appear();
                this.SetCmdRefreshBinding();
            }
        }

        #endregion Methods
    }
}
