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
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.Utils.Extensions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsLoadingUnitsMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly BindingList<MainNavigationMenuItem> cellItems = new BindingList<MainNavigationMenuItem>();

        private readonly IHealthProbeService healthProbeService;

        private readonly BindingList<MainNavigationMenuItem> lodingUnitItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineModeService machineModeService;

        private readonly IMachineSetupStatusWebService setupStatusWebService;

        private bool areItemsEnabled;

        #endregion

        #region Constructors

        public CellsLoadingUnitsMenuViewModel(
            IMachineSetupStatusWebService setupStatusWebService,
            IMachineModeService machineModeService,
            IHealthProbeService healthProbeService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.setupStatusWebService = setupStatusWebService ?? throw new ArgumentNullException(nameof(setupStatusWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.InitializeData();
        }

        #endregion

        #region Properties

        public BindingList<MainNavigationMenuItem> CellItems => this.cellItems;

        public override EnableMask EnableMask => EnableMask.Any;

        public BindingList<MainNavigationMenuItem> LodingUnitItems => this.lodingUnitItems;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

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
                case InstallatorMenuTypes.Cell:
                    this.cellItems.Add(menuItem);
                    break;

                case InstallatorMenuTypes.LoadingUnit:
                    this.lodingUnitItems.Add(menuItem);
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

            return setupStepStatus ?? new SetupStepStatus { IsCompleted = false, CanBePerformed = true };
        }

        private void InitializeData()
        {
            this.CellItems.Clear();
            this.LodingUnitItems.Clear();

            this.areItemsEnabled = this.machineModeService.MachinePower is MachinePowerState.Powered;

            var values = Enum.GetValues(typeof(CellsLoadingUnitsMenus));
            foreach (CellsLoadingUnitsMenus enumValue in values)
            {
                var viewAttribute = enumValue.GetAttributeOfType<CellsLoadingUnitsMenus, ViewAttribute>();
                var dispAttribute = enumValue.GetAttributeOfType<CellsLoadingUnitsMenus, DisplayAttribute>();

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

                if (this.healthProbeService.HealthMasStatus == HealthStatus.Healthy || this.healthProbeService.HealthMasStatus == HealthStatus.Degraded)
                {
                    var setupStatus = this.MachineService.SetupStatus;
                    foreach (var menuItem in this.cellItems)
                    {
                        var itemStatus = this.GetItemStatus(menuItem, setupStatus);
                        menuItem.IsEnabled = itemStatus.CanBePerformed && this.areItemsEnabled;
                        menuItem.IsActive = itemStatus.IsCompleted && this.areItemsEnabled;
                        menuItem.IsEnabled = this.areItemsEnabled;
                    }
                }

                this.RaisePropertyChanged(nameof(this.CellItems));
                this.RaisePropertyChanged(nameof(this.LodingUnitItems));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
