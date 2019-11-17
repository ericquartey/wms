using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CommonServiceLocator;
using DevExpress.Xpf.Core;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public class PpcDialogView : DXWindow, INavigableView
    {
        #region Fields

        public static readonly DependencyProperty FocusedStartProperty = DependencyProperty.Register(
            nameof(FocusedStart),
            typeof(string),
            typeof(PpcDialogView),
            new FrameworkPropertyMetadata(default(string), null));

        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            nameof(IsClosed),
            typeof(bool),
            typeof(PpcDialogView),
            new PropertyMetadata(ClosedChanged));

        private readonly INavigationService
            navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion

        #region Constructors

        protected PpcDialogView()
        {
            this.Loaded += async (sender, e) => await this.View_Loaded(sender, e);
        }

        #endregion

        #region Properties

        public string FocusedStart
        {
            get => (string)this.GetValue(FocusedStartProperty);
            set => this.SetValue(FocusedStartProperty, value);
        }

        public bool IsClosed
        {
            get => (bool)this.GetValue(IsClosedProperty);
            set => this.SetValue(IsClosedProperty, value);
        }

        #endregion

        #region Methods

        public static void SetFocus(INavigableView view, string controlNameToFocus)
        {
            if (string.IsNullOrEmpty(controlNameToFocus) == false &&
                view is DependencyObject viewDep)
            {
                var elemToFocus = viewDep.Descendants<FrameworkElement>()
                                         .FirstOrDefault(item => item.Name == controlNameToFocus);
                if (elemToFocus != null)
                {
                    elemToFocus.Focus();
                }
            }
        }

        public static void ShowAnchorDialog(INavigableView registeredView, bool isNoModalDialog = false, bool isChildOfMainWindow = true)
        {
            if (!(registeredView is PpcDialogView ppcDialog))
            {
                return;
            }

            if (Application.Current.MainWindow.IsVisible
                &&
                isChildOfMainWindow)
            {
                ppcDialog.Owner = Application.Current.MainWindow;
                ppcDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                ppcDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ppcDialog.IsDraggingOrResizing = true;

            if (isNoModalDialog)
            {
                ppcDialog.Show();
            }
            else
            {
                ppcDialog.ShowDialog();
            }
        }

        public static void ShowDialog(INavigableView registeredView, bool isNoModalDialog = false, bool isChildOfMainWindow = true)
        {
            if (!(registeredView is PpcDialogView ppcDialog))
            {
                return;
            }

            if (Application.Current.MainWindow.IsVisible
                &&
                isChildOfMainWindow)
            {
                ppcDialog.Owner = Application.Current.MainWindow;
                ppcDialog.SetSize(new Size(1024, 768));
                ppcDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                ppcDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (isNoModalDialog)
            {
                ppcDialog.Show();
            }
            else
            {
                ppcDialog.ShowDialog();
            }
        }

        public void Disappear()
        {
            this.IsClosed = true;

            ((INavigableViewModel)this.DataContext).Disappear();
            ((INavigableViewModel)this.DataContext).Dispose();

            this.Close();
            if (this.Owner == null &&
                Application.Current.MainWindow.IsVisible == false)
            {
                Application.Current.Shutdown();
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            this.Disappear();
        }

        private static void ClosedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcDialogView dialogView &&
                e.NewValue is bool isClosed &&
                isClosed)
            {
                dialogView.Disappear();
            }
        }

        private void CheckDataContext()
        {
        }

        private string GetAttachedViewModel()
        {
            return $"{this.GetType()}{VW.Utils.Common.MODEL_SUFFIX}";
        }

        private bool IsWrongDataContext()
        {
            if (this.DataContext == null)
            {
                return true;
            }

            var dataContextName = this.DataContext.GetType().ToString();
            return !this.GetAttachedViewModel().Equals(dataContextName, StringComparison.Ordinal);
        }

        private void LoadTheme()
        {
            if (this.Owner == null)
            {
                return;
            }

            var themeName = ServiceLocator.Current.GetInstance<IThemeService>().ActiveTheme;
            var dictionary = new ResourceDictionary();
            var resourceUri =
                $"pack://application:,,,/{VW.Utils.Common.ASSEMBLY_THEMENAME};Component/Skins/{themeName}/{themeName}DialogPopup.xaml";
            dictionary.Source = new Uri(resourceUri, UriKind.Absolute);
            this.Resources.MergedDictionaries.Add(dictionary);
        }

        private async Task View_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.IsWrongDataContext() == false)
            {
                return;
            }

            this.LoadTheme();

            if (this.DataContext is IActivationViewModel activationViewModel)
            {
                this.SetBinding(
                    IsEnabledProperty,
                    new Binding(nameof(IActivationViewModel.IsEnabled)) { Source = activationViewModel });
            }

            if (this.DataContext is INavigableViewModel viewModel)
            {
                await viewModel.OnAppearedAsync();
            }

            SetFocus(this, this.FocusedStart);
        }

        #endregion
    }
}
