using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsDialogView : DXWindow, INavigableView
    {
        #region Fields

        public static readonly DependencyProperty FocusedStartProperty = DependencyProperty.Register(
            nameof(FocusedStart),
            typeof(string),
            typeof(WmsDialogView),
            new FrameworkPropertyMetadata(default(string), null));

        public static readonly DependencyProperty HeaderIsEnabledProperty = DependencyProperty.Register(
            nameof(HeaderIsEnabled),
            typeof(bool),
            typeof(WmsDialogView),
            new FrameworkPropertyMetadata(false, OnHeaderIsEnabledChanged));

        private readonly INavigationService
                    navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly WmsViewType viewType;

        #endregion Fields

        #region Constructors

        protected WmsDialogView()
        {
            var dictionary = new ResourceDictionary();
            var resourceUri = $"pack://application:,,,/{Utils.Common.ASSEMBLY_THEMENAME};Component/Themes/{Utils.Common.THEME_DEFAULTNAME}/{nameof(WmsDialogView)}.xaml";
            dictionary.Source = new Uri(resourceUri, UriKind.Absolute);
            this.Resources.MergedDictionaries.Add(dictionary);
            this.viewType = WmsViewType.Dialog;
            this.Loaded += this.WMSView_Loaded;
        }

        #endregion Constructors

        #region Properties

        public object Data { get; set; }

        public string FocusedStart
        {
            get => (string)this.GetValue(FocusedStartProperty);
            set => this.SetValue(FocusedStartProperty, value);
        }

        public bool HeaderIsEnabled
        {
            get => (bool)this.GetValue(HeaderIsEnabledProperty);
            set => this.SetValue(HeaderIsEnabledProperty, value);
        }

        public bool IsClosed { get; set; }

        public string MapId { get; set; }

        public string Token { get; set; }

        public WmsViewType ViewType => this.viewType;

        #endregion Properties

        #region Methods

        public static void ShowDialog(INavigableView registeredView)
        {
            if (!(registeredView is WmsDialogView wmsDialog))
            {
                return;
            }

            if (Application.Current.MainWindow.IsVisible)
            {
                wmsDialog.Owner = Application.Current.MainWindow;
                wmsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                wmsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            wmsDialog.ShowDialog();
        }

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
                this.navigationService.Disappear(this);
                ((INavigableViewModel)this.DataContext).Dispose();

                this.Close();
                if (this.Owner == null &&
                    Application.Current.MainWindow.IsVisible == false)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        protected override void OnClose()
        {
            base.OnClose();

            this.Disappear();
        }

        private static void EnableControls(WmsDialogView dialogView, bool isEnabled)
        {
            var childrenToCheck = LayoutTreeHelper.GetVisualChildren(dialogView).OfType<IEnabled>();
            if (childrenToCheck != null)
            {
                childrenToCheck.ForEach(c => c.IsEnabled = isEnabled);
            }
        }

        private static void OnHeaderIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsDialogView dialogView && e.NewValue is bool isEnabled)
            {
                EnableControls(dialogView, isEnabled);
            }
        }

        private void CheckDataContext()
        {
            if (this.IsWrongDataContext() == false)
            {
                return;
            }

            this.DataContext = string.IsNullOrEmpty(this.MapId) == false ?
                this.navigationService.GetRegisteredViewModel(this.MapId, this.Data) :
                this.navigationService.RegisterAndGetViewModel(this.GetType().ToString(), this.GetMainViewToken(), this.Data);

            ((INavigableViewModel)this.DataContext)?.Appear();
            FormControl.SetFocus(this, this.FocusedStart);
        }

        private string GetAttachedViewModel()
        {
            return $"{this.GetType().ToString()}{Utils.Common.MODEL_SUFFIX}";
        }

        private string GetMainViewToken()
        {
            string token = null;
            var parentMainView = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is WmsDialogView);
            if (parentMainView != null)
            {
                token = ((WmsDialogView)parentMainView).Token;
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
        }

        #endregion Methods
    }
}
