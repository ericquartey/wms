using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    internal class ItemsAndDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private bool isDetailsViewVisible;

        #endregion Fields

        #region Constructors

        public ItemsAndDetailsViewModel()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Subscribe((ShowDetailsEventArgs<Item> eventArgs) =>
                {
                    this.IsDetailsViewVisible = eventArgs.IsDetailsViewVisible;
                });
        }

        #endregion Constructors

        #region Properties

        public bool IsDetailsViewVisible
        {
            get => this.isDetailsViewVisible;
            set => this.SetProperty(ref this.isDetailsViewVisible, value);
        }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            this.IsDetailsViewVisible = false;
        }

        #endregion Methods
    }
}
