using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    internal class LoadingUnitsAndDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private bool isDetailsViewVisible;
        private string selectedId;

        #endregion Fields

        #region Constructors

        public LoadingUnitsAndDetailsViewModel()
        {
            this.Initialize();
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

        private void Initialize()
        {
            this.eventService.Subscribe((ShowDetailsEventArgs<LoadingUnit> eventArgs) =>
            {
                if (this.Token != eventArgs.Token)
                {
                    return;
                }
                this.IsDetailsViewVisible = eventArgs.IsDetailsViewVisible;
                this.StateId = this.IsDetailsViewVisible ? this.selectedId : string.Empty;
            });

            this.eventService.Subscribe((ItemSelectionChangedEvent<LoadingUnitDetails> eventArgs) =>
            {
                if (this.Token != eventArgs.Token)
                {
                    return;
                }
                if (eventArgs.SelectedItem == null)
                {
                    this.selectedId = null;
                }
                else
                {
                    this.selectedId = eventArgs.SelectedItem.Id.ToString();
                }
            });
        }

        #endregion Methods
    }
}
