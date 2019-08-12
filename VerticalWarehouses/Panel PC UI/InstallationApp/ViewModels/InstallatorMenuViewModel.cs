using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Attributes;
using Ferretto.VW.App.Installation.Models;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class InstallatorMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IInstallationStatusMachineService installationStatusService;

        private readonly BindingList<MainNavigationMenuItem> installatorItems = new BindingList<MainNavigationMenuItem>();

        private readonly BindingList<MainNavigationMenuItem> otherItems = new BindingList<MainNavigationMenuItem>();

        private readonly BindingList<MainNavigationMenuItem> sensorsItems = new BindingList<MainNavigationMenuItem>();

        #endregion

        #region Constructors

        public InstallatorMenuViewModel(IInstallationStatusMachineService installationStatusService)
            : base(PresentationMode.Installator)
        {
            this.InitializeData();
            this.installationStatusService = installationStatusService;
        }

        #endregion

        #region Properties

        public BindingList<MainNavigationMenuItem> InstallatorItems => this.installatorItems;

        public BindingList<MainNavigationMenuItem> OtherItems => this.otherItems;

        public BindingList<MainNavigationMenuItem> SensorsItems => this.sensorsItems;

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();
            this.SohwBack(false);

            try
            {
                var installationStatus = await this.installationStatusService.GetStatusAsync();
                var checkHomingDone = installationStatus.FirstOrDefault();

                this.EnableMenuItem(this.installatorItems, InstallatorMenus.VerticalOffsetCalibration, checkHomingDone);
            }
            catch (Exception ex)
            {
                this.EnableMenuItem(this.installatorItems, InstallatorMenus.VerticalOffsetCalibration, false);
                this.ShowError(ex);
            }
        }

        private void AddMenuItem(InstallatorMenuTypes menuType, MainNavigationMenuItem menuItem)
        {
            switch (menuType)
            {
                case InstallatorMenuTypes.None:
                    break;

                case InstallatorMenuTypes.Installator:
                    this.installatorItems.Add(menuItem);
                    break;

                case InstallatorMenuTypes.Sensors:
                    this.sensorsItems.Add(menuItem);
                    break;

                case InstallatorMenuTypes.Others:
                    this.otherItems.Add(menuItem);
                    break;

                default:
                    break;
            }
        }

        private void EnableMenuItem(IEnumerable<MainNavigationMenuItem> menuItems, InstallatorMenus menuItemType, bool isEnabled)
        {
            if (menuItems.FirstOrDefault(i => i.MenuItemType == menuItemType) is MainNavigationMenuItem menuItem)
            {
                menuItem.IsEnabled = isEnabled;
            }
        }

        private void InitializeData()
        {
            this.InstallatorItems.Clear();
            this.SensorsItems.Clear();
            this.OtherItems.Clear();

            var values = Enum.GetValues(typeof(InstallatorMenus));
            foreach (InstallatorMenus enumValue in values)
            {
                var viewAttribute = enumValue.GetAttributeOfType<InstallatorMenus, ViewAttribute>();
                var dispAttribute = enumValue.GetAttributeOfType<InstallatorMenus, DisplayAttribute>();

                if (viewAttribute != null
                    &&
                    dispAttribute != null)
                {
                    this.AddMenuItem(viewAttribute.InstallatorMenuType,
                        new MainNavigationMenuItem(enumValue, viewAttribute.ViewModelName, viewAttribute.ModuleName, dispAttribute.Description, viewAttribute.CanBackTrack));
                }
            }

            this.RaisePropertyChanged(nameof(this.InstallatorItems));
            this.RaisePropertyChanged(nameof(this.SensorsItems));
            this.RaisePropertyChanged(nameof(this.OtherItems));
        }

        #endregion
    }
}
