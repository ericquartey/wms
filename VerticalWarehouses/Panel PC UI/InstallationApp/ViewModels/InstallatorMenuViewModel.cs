using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        private readonly List<NavigationMenuItem> installatorItems = new List<NavigationMenuItem>();

        private readonly List<NavigationMenuItem> otherItems = new List<NavigationMenuItem>();

        private readonly List<NavigationMenuItem> sensorsItems = new List<NavigationMenuItem>();

        #endregion

        #region Constructors

        public InstallatorMenuViewModel()
            : base(PresentationMode.Installator)
        {
        }

        #endregion

        #region Properties

        public List<NavigationMenuItem> InstallatorItems => this.installatorItems;

        public List<NavigationMenuItem> OtherItems => this.otherItems;

        public List<NavigationMenuItem> SensorsItems => this.sensorsItems;

        #endregion

        #region Methods

        public override void OnNavigated()
        {
            base.OnNavigated();

            this.InitializeData();
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
