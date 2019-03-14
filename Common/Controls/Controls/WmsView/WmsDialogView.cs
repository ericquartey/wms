using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.Common.Controls.Interfaces;
using CommonServiceLocator;

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

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            nameof(Mode),
            typeof(WmsDialogType),
            typeof(WmsDialogView),
            new FrameworkPropertyMetadata(WmsDialogType.None));

        private readonly INavigationService
                    navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly WmsViewType viewType;

        #endregion

        #region Constructors

        protected WmsDialogView()
        {
            this.viewType = WmsViewType.Dialog;
            this.Loaded += this.WMSView_Loaded;
        }

        #endregion

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

        public WmsDialogType Mode
        {
            get => (WmsDialogType)this.GetValue(ModeProperty);
            set => this.SetValue(ModeProperty, value);
        }

        public string Token { get; set; }

        public WmsViewType ViewType => this.viewType;

        #endregion

        #region Methods

        public static void ShowDialog(INavigableView registeredView, bool isNoModalDialog = false)
        {
            if (!(registeredView is WmsDialogView wmsDialog))
            {
                return;
            }

            if (Application.Current.MainWindow.IsVisible)
            {
                wmsDialog.Owner = Application.Current.MainWindow;
                wmsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (wmsDialog.Mode == WmsDialogType.DialogWindow)
                {
                    if (wmsDialog.Owner.WindowState != WindowState.Maximized)
                    {
                        wmsDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                        wmsDialog.Width = wmsDialog.Owner.ActualWidth;
                        wmsDialog.Height = wmsDialog.Owner.ActualHeight;
                        wmsDialog.Top = wmsDialog.Owner.Top;
                        wmsDialog.Left = wmsDialog.Owner.Left;
                    }
                    else
                    {
                        wmsDialog.WindowState = WindowState.Maximized;
                    }
                }
            }
            else
            {
                wmsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (isNoModalDialog)
            {
                wmsDialog.Show();
            }
            else
            {
                wmsDialog.ShowDialog();
            }
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

        private string GetThemeNameFromMode()
        {
            string theme = null;
            switch (this.Mode)
            {
                case WmsDialogType.DialogPopup:
                    theme = nameof(WmsDialogType.DialogPopup);
                    break;

                case WmsDialogType.DialogWindow:
                    theme = nameof(WmsDialogType.DialogWindow);
                    break;

                default:
                    theme = nameof(WmsDialogType.DialogWindow);
                    break;
            }

            return theme;
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

        private void LoadTheme(string theme)
        {
            var dictionary = new ResourceDictionary();
            var resourceUri = $"pack://application:,,,/{Utils.Common.ASSEMBLY_THEMENAME};Component/Themes/{Utils.Common.THEME_DEFAULTNAME}/{theme}.xaml";
            dictionary.Source = new Uri(resourceUri, UriKind.Absolute);
            this.Resources.MergedDictionaries.Add(dictionary);
        }

        private void WMSView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CheckDataContext();

            this.LoadTheme(this.GetThemeNameFromMode());
        }

        #endregion
    }
}
