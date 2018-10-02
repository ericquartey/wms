using DevExpress.Xpf.Core;
using Ferretto.Common.Controls;

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
            if (DXSplashScreen.IsActive)
            {
                DXSplashScreen.Close();
            }
        }

        #endregion Methods
    }
}
