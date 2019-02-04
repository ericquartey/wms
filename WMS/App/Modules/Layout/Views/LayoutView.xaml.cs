using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.Modules.Layout
{
    public partial class LayoutView : WmsView
    {
        #region Constructors

        public LayoutView()
        {
            this.InitializeComponent();
            this.Loaded += LayoutView_Loaded;
        }

        #endregion

        #region Methods

        private static void LayoutView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SplashScreenService.Hide();
        }

        #endregion
    }
}
