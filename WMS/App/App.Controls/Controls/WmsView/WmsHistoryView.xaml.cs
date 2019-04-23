using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommonServiceLocator;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    public partial class WmsHistoryView : ContentControl, IWmsHistoryView
    {
        #region Fields

        public static readonly DependencyProperty IsHeaderActionBarVisibleProperty = DependencyProperty.Register(nameof(IsHeaderActionBarVisible), typeof(bool), typeof(WmsHistoryView));

        public static readonly DependencyProperty IsVisibleBackButtonProperty = DependencyProperty.Register(nameof(IsVisibleBackButton), typeof(bool), typeof(WmsHistoryView));

        public static readonly DependencyProperty StartModuleNameProperty = DependencyProperty.Register(nameof(StartModuleName), typeof(string), typeof(WmsHistoryView));

        public static readonly DependencyProperty StartViewNameProperty = DependencyProperty.Register(nameof(StartViewName), typeof(string), typeof(WmsHistoryView));

        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register(nameof(SubTitle), typeof(string), typeof(WmsHistoryView));

        public static readonly DependencyProperty SubTitleVisibilityProperty = DependencyProperty.Register(
                               nameof(SubTitleVisibility), typeof(Visibility), typeof(WmsHistoryView), new FrameworkPropertyMetadata(Visibility.Visible));

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly Stack<INavigableView> registeredViews = new Stack<INavigableView>();

        private INavigableView parentView;

        private INavigableView viewToAdd;

        #endregion

        #region Constructors

        public WmsHistoryView()
        {
            this.InitializeComponent();
        }

        public WmsHistoryView(INavigableView view, INavigableView parentView)
            : this()
        {
            this.viewToAdd = view;
            this.parentView = parentView;
        }

        #endregion

        #region Properties

        public bool IsHeaderActionBarVisible
        {
            get => (bool)this.GetValue(IsHeaderActionBarVisibleProperty);
            set => this.SetValue(IsHeaderActionBarVisibleProperty, value);
        }

        public bool IsVisibleBackButton
        {
            get => (bool)this.GetValue(IsVisibleBackButtonProperty);
            set => this.SetValue(IsVisibleBackButtonProperty, value);
        }

        public string StartModuleName
        {
            get => (string)this.GetValue(StartModuleNameProperty);
            set => this.SetValue(StartModuleNameProperty, value);
        }

        public string StartViewName
        {
            get => (string)this.GetValue(StartViewNameProperty);
            set => this.SetValue(StartViewNameProperty, value);
        }

        public string SubTitle
        {
            get => (string)this.GetValue(SubTitleProperty);
            set => this.SetValue(SubTitleProperty, value);
        }

        public Visibility SubTitleVisibility
        {
            get => (Visibility)this.GetValue(SubTitleVisibilityProperty);
            set => this.SetValue(SubTitleVisibilityProperty, value);
        }

        #endregion

        #region Methods

        public void AddView(INavigableView registeredView)
        {
            this.registeredViews.Push(registeredView);
            this.DataContext = null;
            this.Content = registeredView;
            this.CheckBackVisibility();
            this.ReAssignBindings(registeredView);
        }

        public void Appear(string moduleName, string viewModelName, object data)
        {
            if (string.IsNullOrEmpty(viewModelName))
            {
                return;
            }

            var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
            var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);
            var instanceModuleViewName = this.navigationService.GetNewViewModelName(moduleViewName);
            var registeredView = this.navigationService.GetView(instanceModuleViewName, data);
            this.AddView(registeredView);
        }

        public bool CanDisappear(RoutedEventArgs args = null)
        {
            if (this.Content is WmsView wmsView &&
                wmsView.DataContext is INavigableViewModel viewModel)
            {
                if (args != null)
                {
                    args.Handled = true;
                }

                return viewModel.CanDisappear();
            }

            return true;
        }

        public INavigableViewModel GetCurrentViewModel()
        {
            if (this.Content == null)
            {
                return null;
            }

            return ((FrameworkElement)this.Content).DataContext as INavigableViewModel;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (string.IsNullOrEmpty(this.StartModuleName) == false &&
                string.IsNullOrEmpty(this.StartViewName) == false)
            {
                var data = this.GetParentWmsViewData();
                this.Appear(this.StartModuleName, this.StartViewName, data);
            }

            if (this.viewToAdd != null)
            {
                this.AddView(this.viewToAdd);
            }
        }

        public void Previous()
        {
            if (this.registeredViews.Count == 1)
            {
                return;
            }

            var poppedView = this.registeredViews.Pop();
            this.Content = this.registeredViews.Peek();
            this.CheckBackVisibility();
            this.ReAssignBindings(this.Content as INavigableView);

            if (poppedView is INavigableView view)
            {
                view.Disappear();
            }
        }

        private void BackViewClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if (this.CanDisappear())
            {
                this.Previous();
            }
        }

        private void CheckBackVisibility()
        {
            this.IsHeaderActionBarVisible = (this.registeredViews.Count == 1) == false;
        }

        private void CreateBinding(WmsView source, string bindingName, DependencyProperty depPropertyName)
        {
            var myBinding = new Binding();
            myBinding.Source = source;
            myBinding.Path = new PropertyPath(path: bindingName);
            myBinding.Mode = BindingMode.TwoWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(this, depPropertyName, myBinding);
        }

        private object GetParentWmsViewData()
        {
            var parentWmsView = LayoutTreeHelper.GetVisualParents(this)
               .OfType<WmsView>()
               .FirstOrDefault();

            return parentWmsView?.Data;
        }

        private void ReAssignBindings(INavigableView navigableView)
        {
            var wmsView = navigableView;
            if (this.parentView != null &&
                this.registeredViews.Count == 1)
            {
                wmsView = this.parentView;
            }

            if (wmsView is WmsView view)
            {
                this.CreateBinding(view, nameof(WmsView.IsVisibleBackButton), WmsHistoryView.IsVisibleBackButtonProperty);
                this.CreateBinding(view, nameof(WmsView.SubTitle), WmsHistoryView.SubTitleProperty);
                this.CreateBinding(view, nameof(WmsView.SubTitleVisibility), WmsHistoryView.SubTitleVisibilityProperty);
            }
        }

        #endregion
    }
}
