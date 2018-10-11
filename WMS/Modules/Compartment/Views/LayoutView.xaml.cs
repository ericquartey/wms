using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.Modules.Compartment
{
    public partial class LayoutView : WmsView
    {
        #region Constructors

        public LayoutView()
        {
            this.InitializeComponent();
            this.Loaded += this.LayoutView_Loaded;
        }

        #endregion Constructors

        #region Methods

        private void LayoutView_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SplashScreenService.Hide();
        }

        #endregion Methods
    }
}
