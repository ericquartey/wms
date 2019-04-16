using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.Utils.Menu;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Interfaces;

namespace Ferretto.WMS.Modules.Layout
{
    public class MenuViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IAuthenticationProvider authenticationProvider = ServiceLocator.Current.GetInstance<IAuthenticationProvider>();

        private readonly ObservableCollection<NavMenuItem> menuItems = new ObservableCollection<NavMenuItem>();

        private string userName;

        #endregion

        #region Properties

        public ObservableCollection<NavMenuItem> Items => this.menuItems;

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        #endregion

        #region Methods

        protected override async Task OnAppearAsync()
        {
            await this.InizializeAsync();
        }

        private async Task InizializeAsync()
        {
            var menu = new AppMenu();
            foreach (var item in menu.Menu.Items)
            {
                this.Items.Add(new NavMenuItem(item, string.Empty));
            }

            this.UserName = await this.authenticationProvider.GetUserNameAsync();
        }

        #endregion
    }
}
