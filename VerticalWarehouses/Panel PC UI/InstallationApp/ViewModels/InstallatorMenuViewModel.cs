using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.Attributes;
using Ferretto.VW.App.Installation.Models;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class InstallatorMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly BindingList<NavigationMenuItem> installatorItems = new BindingList<NavigationMenuItem>();

        private readonly BindingList<NavigationMenuItem> otherItems = new BindingList<NavigationMenuItem>();

        private readonly BindingList<NavigationMenuItem> sensorsItems = new BindingList<NavigationMenuItem>();

        #endregion

        #region Constructors

        public InstallatorMenuViewModel()
            : base(PresentationMode.Installator)
        {
            this.InitializeData();
        }

        #endregion

        #region Properties

        public BindingList<NavigationMenuItem> InstallatorItems => this.installatorItems;

        public BindingList<NavigationMenuItem> OtherItems => this.otherItems;

        public BindingList<NavigationMenuItem> SensorsItems => this.sensorsItems;

        #endregion

        #region Methods

        public override void OnNavigated()
        {
            base.OnNavigated();

            var state = new Presentation()
            {
                Type = PresentationTypes.Back,
                IsVisible = false
            };

            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()?.Publish(new PresentationChangedMessage(state));
        }

        private void AddMenuItem(InstallatorMenuTypes menuType, NavigationMenuItem menuItem)
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

                this.AddMenuItem(viewAttribute.InstallatorMenuType,
                                new NavigationMenuItem(viewAttribute.ViewModelName, viewAttribute.ModuleName, dispAttribute.Description));
            }

            this.RaisePropertyChanged(nameof(this.InstallatorItems));
            this.RaisePropertyChanged(nameof(this.SensorsItems));
            this.RaisePropertyChanged(nameof(this.OtherItems));
        }

        #endregion
    }
}
