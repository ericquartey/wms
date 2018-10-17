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

        public static readonly DependencyProperty EnableHistoryViewProperty = DependencyProperty.Register(nameof(EnableHistoryView), typeof(bool), typeof(WmsView));

        private readonly INavigationService
            navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly WmsViewType viewType;
        private WmsHistoryView wmsHistoryView;

        #endregion Fields

        #region Constructors

        protected WmsView()
        {
            this.viewType = WmsViewType.Docking;
            this.Loaded += this.WMSView_Loaded;
        }

        #endregion Constructors

        #region Properties

        public object Data { get; set; }

        public bool EnableHistoryView
        {
            get => (bool)this.GetValue(EnableHistoryViewProperty);
            set => this.SetValue(EnableHistoryViewProperty, value);
        }

        public bool IsClosed { get; set; }

        public string MapId { get; set; }

        public string Title { get; set; }
        public string Token { get; set; }
        public WmsViewType ViewType => this.viewType;

        #endregion Properties

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
            var clonedView = new WmsView()
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

            this.CheckToAddHistoryView();
            ((INavigableViewModel)this.DataContext)?.Appear();
        }

        #endregion Methods
    }
}
