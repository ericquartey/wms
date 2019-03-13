using System.Windows.Controls;
using DevExpress.Xpf.Bars;

namespace Ferretto.Common.Controls
{
    public class WmsStatusBar : StatusBarControl
    {
        #region Constructors

        public WmsStatusBar()
        {
            this.AllowCustomizationMenu = false;
            this.DataContext = new StatusBarViewModel();
        }

        #endregion
    }
}
