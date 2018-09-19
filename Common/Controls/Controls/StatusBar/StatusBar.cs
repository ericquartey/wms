using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public class StatusBar : ContentControl
    {
        public StatusBar()
        {
            this.DataContext = new StatusBarViewModel();
        }
    }
}
