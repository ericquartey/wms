using CommonServiceLocator;
using DevExpress.Xpf.Bars;

namespace Ferretto.WMS.App.Controls
{
    public class WmsStatusBar : StatusBarControl
    {
        #region Constructors

        public WmsStatusBar()
        {
            this.AllowCustomizationMenu = false;
            this.DataContext = ServiceLocator.Current.GetInstance<StatusBarViewModel>();
        }

        #endregion
    }
}
