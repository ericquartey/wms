using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Operator.Attributes;
using Ferretto.VW.App.Operator.Models;
using Ferretto.VW.App.Operator.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils;
using Ferretto.VW.Utils.Extensions;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class OperatorMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly BindingList<MainNavigationMenuItem> installatorItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineModeService machineModeService;

        private readonly BindingList<MainNavigationMenuItem> operatorItems = new BindingList<MainNavigationMenuItem>();

        private readonly IMachineSetupStatusWebService setupStatusWebService;

        private bool areItemsEnabled;

        private int bayNumber;

        #endregion

        #region Constructors

        public OperatorMenuViewModel(
            IMachineSetupStatusWebService setupStatusWebService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            if (bayManager is null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.setupStatusWebService = setupStatusWebService ?? throw new ArgumentNullException(nameof(setupStatusWebService));
            this.machineModeService = machineModeService ?? throw new ArgumentNullException(nameof(machineModeService));
            this.bayManager = bayManager;
            this.InitializeData();
        }

        #endregion

        #region Properties

        public bool AreItemsEnabled
        {
            get => this.areItemsEnabled;
            private set => this.SetProperty(ref this.areItemsEnabled, value);
        }

        public override EnableMask EnableMask => EnableMask.None;

        public BindingList<MainNavigationMenuItem> InstallatorItems => this.installatorItems;

        public BindingList<MainNavigationMenuItem> OperatorItems => this.operatorItems;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = false;

            var bay = await this.bayManager.GetBayAsync();
            this.bayNumber = (int)bay.Number;

            await this.UpdateMenuItemsStatus();
        }

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            this.AreItemsEnabled = e.MachinePower != Services.Models.MachinePowerState.Unpowered;
        }

        private void AddMenuItem(OperatorMenuTypes menuType, MainNavigationMenuItem menuItem)
        {
            switch (menuType)
            {
                case OperatorMenuTypes.Installer:
                    this.installatorItems.Add(menuItem);
                    break;

                case OperatorMenuTypes.Operator:
                    this.operatorItems.Add(menuItem);
                    break;
            }
        }

        private void EnableMenuItem(IEnumerable<MainNavigationMenuItem> menuItems, OperatorMenus menuItemType, bool isEnabled)
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

            return setupStepStatus ?? new SetupStepStatus { IsCompleted = false, CanBePerformed = false };
        }

        private void InitializeData()
        {
            this.InstallatorItems.Clear();
            this.OperatorItems.Clear();

            var values = Enum.GetValues(typeof(OperatorMenus));
            foreach (OperatorMenus enumValue in values)
            {
                var viewAttribute = enumValue.GetAttributeOfType<OperatorMenus, ViewAttribute>();
                var dispAttribute = enumValue.GetAttributeOfType<OperatorMenus, DisplayAttribute>();

                if (viewAttribute != null
                    &&
                    dispAttribute != null)
                {
                    this.AddMenuItem(
                        viewAttribute.OperatorMenuType,
                        new MainNavigationMenuItem(enumValue, viewAttribute.ViewModelName, viewAttribute.ModuleName, dispAttribute.Description, trackCurrentView: true));
                }
            }

            this.AreItemsEnabled = this.machineModeService.MachinePower != Services.Models.MachinePowerState.Unpowered;

            this.RaisePropertyChanged(nameof(this.InstallatorItems));
            this.RaisePropertyChanged(nameof(this.OperatorItems));
        }

        private async Task UpdateMenuItemsStatus()
        {
            try
            {
                var setupStatus = await this.setupStatusWebService.GetAsync();

                foreach (var menuItem in this.operatorItems)
                {
                    var itemStatus = this.GetItemStatus(menuItem, setupStatus);
                    menuItem.IsEnabled = itemStatus.CanBePerformed;
                    menuItem.IsActive = itemStatus.IsCompleted;
                }

                foreach (var menuItem in this.installatorItems)
                {
                    var itemStatus = this.GetItemStatus(menuItem, setupStatus);
                    menuItem.IsEnabled = itemStatus.CanBePerformed;
                    menuItem.IsActive = itemStatus.IsCompleted;
                }

                this.AreItemsEnabled = this.machineModeService.MachinePower != Services.Models.MachinePowerState.Unpowered;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
