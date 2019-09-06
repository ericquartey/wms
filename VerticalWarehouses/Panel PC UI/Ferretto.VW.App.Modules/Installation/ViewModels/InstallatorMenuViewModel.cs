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
using Ferretto.VW.Utils.Extensions;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class InstallatorMenuViewModel : BaseMainViewModel
    {

        #region Fields

        private readonly int bayNumber;

        private readonly BindingList<MainNavigationMenuItem> installatorItems = new BindingList<MainNavigationMenuItem>();

        private readonly BindingList<MainNavigationMenuItem> otherItems = new BindingList<MainNavigationMenuItem>();

        private readonly BindingList<MainNavigationMenuItem> sensorsItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineSetupStatusService setupStatusService;

        #endregion

        #region Constructors

        public InstallatorMenuViewModel(
            IMachineSetupStatusService setupStatusService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            if (setupStatusService is null)
            {
                throw new ArgumentNullException(nameof(setupStatusService));
            }

            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.setupStatusService = setupStatusService;

            //TODO Review Implementation avoid using numbers to identify bays
            this.bayNumber = (int)bayManager.Bay.Index;

            this.InitializeData();
        }

        #endregion



        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

        public BindingList<MainNavigationMenuItem> InstallatorItems => this.installatorItems;

        public BindingList<MainNavigationMenuItem> OtherItems => this.otherItems;

        public BindingList<MainNavigationMenuItem> SensorsItems => this.sensorsItems;

        #endregion



        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = false;

            await this.UpdateMenuItemsStatus();
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            await this.UpdateMenuItemsStatus();
        }

        private void AddMenuItem(InstallatorMenuTypes menuType, MainNavigationMenuItem menuItem)
        {
            switch (menuType)
            {
                case InstallatorMenuTypes.Installer:
                    this.installatorItems.Add(menuItem);
                    break;

                case InstallatorMenuTypes.Sensors:
                    this.sensorsItems.Add(menuItem);
                    break;

                case InstallatorMenuTypes.Others:
                    this.otherItems.Add(menuItem);
                    break;
            }
        }

        private void EnableMenuItem(IEnumerable<MainNavigationMenuItem> menuItems, InstallationMenus menuItemType, bool isEnabled)
        {
            if (menuItems.FirstOrDefault(i => i.MenuItemType == menuItemType) is MainNavigationMenuItem menuItem)
            {
                menuItem.IsEnabled = isEnabled;
            }
        }

        private SetupStepStatus GetItemStatus(MainNavigationMenuItem menuItem, SetupStatusCapabilities setupStatus)
        {
            SetupStepStatus setupStepStatus = null;

            var dictionary = setupStatus.ToDictionary();

            var propertyName = menuItem.MenuItemType.ToString();

            if (dictionary.ContainsKey(propertyName))
            {
                setupStepStatus = dictionary[propertyName] as SetupStepStatus;
            }
            else
            {
                var bayName = $"Bay{this.bayNumber}";
                if (dictionary.ContainsKey(bayName))
                {
                    var baySetupStatus = dictionary[bayName] as BaySetupStatus;
                    var bayDictionary = baySetupStatus.ToDictionary();

                    var bayPropertyName = propertyName.Replace("Bay", "");

                    if (bayDictionary.ContainsKey(bayPropertyName))
                    {
                        setupStepStatus = bayDictionary[bayPropertyName] as SetupStepStatus;
                    }
                }
            }

            return setupStepStatus ?? new SetupStepStatus { IsCompleted = false, CanBePerformed = false };
        }

        private void InitializeData()
        {
            this.InstallatorItems.Clear();
            this.SensorsItems.Clear();
            this.OtherItems.Clear();

            var values = Enum.GetValues(typeof(InstallationMenus));
            foreach (InstallationMenus enumValue in values)
            {
                var viewAttribute = enumValue.GetAttributeOfType<InstallationMenus, ViewAttribute>();
                var dispAttribute = enumValue.GetAttributeOfType<InstallationMenus, DisplayAttribute>();

                if (viewAttribute != null
                    &&
                    dispAttribute != null)
                {
                    this.AddMenuItem(
                        viewAttribute.InstallatorMenuType,
                        new MainNavigationMenuItem(enumValue, viewAttribute.ViewModelName, viewAttribute.ModuleName, dispAttribute.Description, trackCurrentView: true));
                }
            }

            this.RaisePropertyChanged(nameof(this.InstallatorItems));
            this.RaisePropertyChanged(nameof(this.SensorsItems));
            this.RaisePropertyChanged(nameof(this.OtherItems));
        }

        private async Task UpdateMenuItemsStatus()
        {
            try
            {
                var setupStatus = await this.setupStatusService.GetAsync();

                foreach (var menuItem in this.installatorItems)
                {
                    var itemStatus = this.GetItemStatus(menuItem, setupStatus);
                    menuItem.IsEnabled = itemStatus.CanBePerformed;
                    menuItem.IsActive = itemStatus.IsCompleted;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
