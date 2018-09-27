using System.Linq;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsView : UserControl, INavigableView
    {
        #region Fields

        private readonly INavigationService
            navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion Fields

        #region Constructors

        protected WmsView()
        {
            this.Loaded += this.WMSView_Loaded;
        }

        #endregion Constructors

        #region Properties

        public string MapId { get; set; }
        public string Title { get; set; }
        public string Token { get; set; }

        #endregion Properties

        #region Methods

        private string GetAttachedViewModel()
        {
            return $"{this.GetType().ToString()}{Utils.Common.MODEL_SUFFIX}";
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
                this.DataContext = this.navigationService.GetRegisteredViewModel(this.MapId);
            }
            else
            {
                this.DataContext =
                    this.navigationService.RegisterAndGetViewModel(this.GetType().ToString(), this.GetMainViewToken());
            }

            ((INavigableViewModel)this.DataContext)?.Appear();
        }

        #endregion Methods
    }
}
