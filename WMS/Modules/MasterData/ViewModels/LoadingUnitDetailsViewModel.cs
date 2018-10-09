using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private ICommand hideDetailsCommand;
        private LoadingUnitDetails loadingUnit;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
                    (this.hideDetailsCommand = new DelegateCommand(this.ExecuteHideDetailsCommand));

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set => this.SetProperty(ref this.loadingUnit, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        #endregion Properties

        #region Methods

        private void ExecuteHideDetailsCommand()
        {
            this.eventService.Invoke(new ShowDetailsEventArgs<LoadingUnit, int>(this.Token, false));
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.loadingUnitProvider.Save(this.LoadingUnit);

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<LoadingUnitDetails, int>(this.LoadingUnit));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEventArgs(Ferretto.Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.eventService.Subscribe<ItemSelectionChangedEvent<LoadingUnitDetails, int>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.ItemId), true);
        }

        private void OnItemSelectionChanged(object itemId)
        {
            if (itemId == null)
            {
                this.LoadingUnit = null;
                return;
            }
            //this.LoadingUnit = this.businessProvider.GetItemDetails((int)itemId);
        }

        #endregion Methods
    }
}
