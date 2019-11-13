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
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils;
using Ferretto.VW.Utils.Extensions;
using Prism.Regions;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class InstallatorMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IHealthProbeService healthProbeService;

        private readonly BindingList<MainNavigationMenuItem> installatorItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineModeService machineModeService;

        private readonly BindingList<MainNavigationMenuItem> otherItems = new BindingList<MainNavigationMenuItem>();

        private readonly BindingList<MainNavigationMenuItem> sensorsItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineSetupStatusWebService setupStatusWebService;

        private bool areItemsEnabled;

        private int bayNumber;

        #endregion

        #region Constructors

        public InstallatorMenuViewModel(
            IMachineSetupStatusWebService setupStatusWebService,
            IMachineModeService machineModeService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.setupStatusWebService = setupStatusWebService ?? throw new ArgumentNullException(nameof(setupStatusWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.InitializeData();
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public BindingList<MainNavigationMenuItem> InstallatorItems => this.installatorItems;

        public BindingList<MainNavigationMenuItem> OtherItems => this.otherItems;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = false;

            try
            {
                var bay = await this.bayManager.GetBayAsync();
                this.bayNumber = (int)bay.Number;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            await this.UpdateMenuItemsStatusAsync();
        }

        protected override async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await base.OnHealthStatusChangedAsync(e);

            await this.UpdateMenuItemsStatusAsync();
        }

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            await this.UpdateMenuItemsStatusAsync();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            await this.UpdateMenuItemsStatusAsync();
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

                    var bayPropertyName = propertyName.Replace("Bay", string.Empty);

                    if (bayDictionary.ContainsKey(bayPropertyName))
                    {
                        setupStepStatus = bayDictionary[bayPropertyName] as SetupStepStatus;
                    }
                }
            }

            return setupStepStatus ?? new SetupStepStatus { IsCompleted = false, CanBePerformed = true };
        }

        private void InitializeData()
        {
            this.InstallatorItems.Clear();
            this.OtherItems.Clear();

            this.areItemsEnabled = this.machineModeService.MachinePower is MachinePowerState.Powered;

            var values = Enum.GetValues(typeof(InstallationMenus));
            foreach (InstallationMenus enumValue in values)
            {
                var viewAttribute = enumValue.GetAttributeOfType<InstallationMenus, ViewAttribute>();
                var dispAttribute = enumValue.GetAttributeOfType<InstallationMenus, DisplayAttribute>();

                if (viewAttribute != null
                    &&
                    dispAttribute != null)
                {
                    var menuItem = new MainNavigationMenuItem(
                        enumValue,
                        viewAttribute.ViewModelName,
                        viewAttribute.ModuleName,
                        dispAttribute.Description,
                        trackCurrentView: true)
                    { IsEnabled = this.areItemsEnabled };

                    this.AddMenuItem(viewAttribute.InstallatorMenuType, menuItem);
                }
            }
        }

        private async Task UpdateMenuItemsStatusAsync()
        {
            try
            {
                this.areItemsEnabled = this.machineModeService.MachinePower is MachinePowerState.Powered;

                if (this.healthProbeService.HealthStatus == HealthStatus.Healthy)
                {
                    var setupStatus = await this.setupStatusWebService.GetAsync();
                    foreach (var menuItem in this.installatorItems)
                    {
                        var itemStatus = this.GetItemStatus(menuItem, setupStatus);
                        menuItem.IsEnabled = itemStatus.CanBePerformed && this.areItemsEnabled;
                        menuItem.IsActive = itemStatus.IsCompleted && this.areItemsEnabled;
                    }
                }

                foreach (var menuItem in this.otherItems)
                {
                    if (menuItem.ViewModelName == Utils.Modules.Installation.Parameters.PARAMETERS
                        ||
                        menuItem.ViewModelName == Utils.Modules.Installation.Sensors.VERTICALAXIS)
                    {
                        menuItem.IsEnabled = this.healthProbeService.HealthStatus == HealthStatus.Healthy;
                    }
                    else
                    {
                        menuItem.IsEnabled = this.areItemsEnabled;
                    }
                }

                foreach (var menuItem in this.installatorItems)
                {
                    if (menuItem.ViewModelName == Utils.Modules.Installation.Parameters.PARAMETERS
                        ||
                        menuItem.ViewModelName == Utils.Modules.Installation.Sensors.VERTICALAXIS)
                    {
                        menuItem.IsEnabled = this.healthProbeService.HealthStatus == HealthStatus.Healthy;
                    }
                    else
                    {
                        menuItem.IsEnabled = this.areItemsEnabled;
                    }
                }

                this.RaisePropertyChanged(nameof(this.InstallatorItems));
                this.RaisePropertyChanged(nameof(this.OtherItems));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
