using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Modules.BLL.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IBusinessProvider businessProvider = ServiceLocator.Current.GetInstance<IBusinessProvider>();
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private ICommand hideDetailsCommand;
        private ItemDetails item;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
                            (this.hideDetailsCommand = new DelegateCommand(this.ExecuteHideDetailsCommand));

        public ItemDetails Item
        {
            get => this.item;
            set
            {
                if (this.SetProperty(ref this.item, value))
                {
                    // TODO: set compartments
                }
            }
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        #endregion Properties

        #region Methods

        private void ExecuteHideDetailsCommand()
        {
            this.eventService.Invoke(new ShowDetailsEventArgs<Item>(this.Token, false));
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.businessProvider.Save(this.Item);

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<ItemDetails>(this.Item));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEvent(Ferretto.Common.Resources.MasterData.ItemSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.eventService.Subscribe<ItemSelectionChangedEvent<Item>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
        }

        private void OnItemSelectionChanged(Item selectedItem)
        {
            if (selectedItem == null)
            {
                this.Item = null;
                return;
            }
            this.Item = this.businessProvider.GetItemDetails(selectedItem.Id);
        }

        #endregion Methods
    }
}
