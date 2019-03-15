using System.Windows;
using CommonServiceLocator;
using DevExpress.Xpf.Bars;

namespace Ferretto.Common.Controls
{
    public class WmsStatusBar : StatusBarControl
    {
        #region Constructors

        public WmsStatusBar()
        {
            this.AllowCustomizationMenu = false;
            this.DataContext = ServiceLocator.Current.GetInstance<StatusBarViewModel>();
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        #endregion

        #region Methods

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((StatusBarViewModel)this.DataContext).IsSubscriptionActive = (bool)e.NewValue;
        }

        #endregion
    }
}
