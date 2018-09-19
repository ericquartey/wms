using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public class StatusBar : ContentControl
    {
        #region Constructors

        public StatusBar()
        {
            this.DataContext = new StatusBarViewModel();
        }

        #endregion Constructors
    }
}
