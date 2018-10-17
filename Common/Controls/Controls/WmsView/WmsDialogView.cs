using System;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public partial class WmsDialogView : DXWindow, INavigableView
    {
        #region Fields

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
        public bool IsClosed { get; set; }
        public string MapId { get; set; }
        public string Token { get; set; }
        public WmsViewType ViewType => this.viewType;

        #endregion Properties

        #region Methods

        public static void ShowDialog(INavigableView registeredView)
        {
            var wmsDialog = registeredView as WmsDialogView;
            if (wmsDialog == null)
            {
                return;
            }
            wmsDialog.Owner = Application.Current.MainWindow;
            wmsDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            wmsDialog.ShowDialog();
        }

        protected override void OnClose()
        {
            base.OnClose();

            this.Disappear();
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
                this.navigationService.Disappear(this.DataContext as INavigableViewModel);
                ((INavigableViewModel)this.DataContext).Dispose();
            }
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

            var dataContextName = this.DataContext.GetType().ToString();
            return !this.GetAttachedViewModel().Equals(dataContextName, System.StringComparison.InvariantCulture);
        }

        private void WMSView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.IsWrongDataContext() == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.MapId) == false)
            {
                // Is Main WMSView registered
                this.DataContext = this.navigationService.GetRegisteredViewModel(this.MapId, this.Data);
            }
            else
            {
                this.DataContext =
                    this.navigationService.RegisterAndGetViewModel(this.GetType().ToString(), this.GetMainViewToken(), this.Data);
            }

            ((INavigableViewModel)this.DataContext)?.Appear();
        }

        #endregion Methods
    }
}
