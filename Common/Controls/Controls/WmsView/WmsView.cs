using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsView : UserControl, INavigableView
    {
        #region Fields

        public static readonly DependencyProperty EnableHistoryViewProperty = DependencyProperty.Register(
            nameof(EnableHistoryView),
            typeof(bool),
            typeof(WmsView));

        public static readonly DependencyProperty FocusedStartProperty = DependencyProperty.Register(
            nameof(FocusedStart),
            typeof(string),
            typeof(WmsView),
            new FrameworkPropertyMetadata(default(string), null));

        public static readonly DependencyProperty IsVisibleBackButtonProperty = DependencyProperty.Register(
            nameof(IsVisibleBackButton),
            typeof(bool),
            typeof(WmsView),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register(
            nameof(SubTitle), typeof(string), typeof(WmsView)
            );

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(INavigableViewModel), typeof(WmsView));

        private readonly INavigationService
            navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly WmsViewType viewType;

        private WmsHistoryView wmsHistoryView;

        #endregion

        #region Constructors

        protected WmsView()
        {
            this.viewType = WmsViewType.Docking;
            this.Loaded += this.WMSView_Loaded;
        }

        #endregion

        #region Properties

        public object Data { get; set; }

        public bool EnableHistoryView
        {
            get => (bool)this.GetValue(EnableHistoryViewProperty);
            set => this.SetValue(EnableHistoryViewProperty, value);
        }

        public string FocusedStart
        {
            get => (string)this.GetValue(FocusedStartProperty);
            set => this.SetValue(FocusedStartProperty, value);
        }

        public bool IsClosed { get; set; }

        public bool IsVisibleBackButton
        {
            get => (bool)this.GetValue(IsVisibleBackButtonProperty);
            set => this.SetValue(IsVisibleBackButtonProperty, value);
        }

        public string MapId { get; set; }

        public string SubTitle
        {
            get => (string)this.GetValue(SubTitleProperty);
            set => this.SetValue(SubTitleProperty, value);
        }

        public string Title { get; set; }

        public string Token { get; set; }

        public INavigableViewModel ViewModel
        {
            get => (INavigableViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        public WmsViewType ViewType => this.viewType;

        #endregion

        #region Methods

        public void Disappear()
        {
            if (this.IsClosed == false)
            {
                this.IsClosed = true;
                var childViews = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsView>();
                foreach (var childView in childViews)
                {
                    childView.Disappear();
                }

                ((INavigableViewModel)this.DataContext).Disappear();
                this.navigationService.Disappear(this.DataContext as INavigableViewModel);
                ((INavigableViewModel)this.DataContext).Dispose();
            }
        }

        private void CheckDataContext()
        {
            if (this.IsWrongDataContext() == false)
            {
                return;
            }

            this.DataContext = !string.IsNullOrEmpty(this.MapId) ?
                this.navigationService.GetRegisteredViewModel(this.MapId, this.Data) : // Is Main WMSView registered
                this.navigationService.RegisterAndGetViewModel(this.GetType().ToString(), this.GetMainViewToken(), this.Data);

            this.ViewModel = (INavigableViewModel)this.DataContext;
            ((INavigableViewModel)this.DataContext)?.Appear();
            FormControl.SetFocus(this, this.FocusedStart);
        }

        private void CheckToAddHistoryView()
        {
            if (this.wmsHistoryView != null)
            {
                return;
            }

            if (this.EnableHistoryView == false)
            {
                return;
            }

            if (LayoutTreeHelper.GetVisualParents(this).OfType<WmsView>().FirstOrDefault(v => v.EnableHistoryView) != null)
            {
                return;
            }

            var newWmsView = this.GetCloned();
            this.wmsHistoryView = new WmsHistoryView(newWmsView);
            this.Content = this.wmsHistoryView;
        }

        private string GetAttachedViewModel()
        {
            return $"{this.GetType().ToString()}{Utils.Common.MODEL_SUFFIX}";
        }

        private WmsView GetCloned()
        {
            var clonedView = new WmsView
            {
                MapId = this.MapId,
                Data = this.Data,
                DataContext = this.DataContext,
                Token = this.Token,
                Content = this.Content
            };
            return clonedView;
        }

        private string GetMainViewToken()
        {
            string token = null;
            var parentMainView = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsView);
            if (parentMainView != null)
            {
                token = ((WmsView)parentMainView).Token;
            }

            return token;
        }

        private bool IsWrongDataContext()
        {
            if (this.DataContext == null)
            {
                return true;
            }

            if (this.MapId != null && this.DataContext is INavigableViewModel navViewModel &&
                this.MapId.Equals(navViewModel.MapId, System.StringComparison.Ordinal))
            {
                return false;
            }

            var dataContextName = this.DataContext.GetType().ToString();
            return !this.GetAttachedViewModel().Equals(dataContextName, System.StringComparison.Ordinal);
        }

        private void WMSView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CheckDataContext();
            this.CheckToAddHistoryView();
        }

        #endregion
    }
}
