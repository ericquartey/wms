using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private ICommand hideDetailsCommand;
        private Item item;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<AbcClass> AbcClassChoices => this.dataService.GetData<AbcClass>().AsEnumerable();

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
            (this.hideDetailsCommand = new DelegateCommand(this.ExecuteHideDetailsCommand));

        public Item Item
        {
            get => this.item;
            set => this.SetProperty(ref this.item, value);
        }

        public IEnumerable<ItemManagementType> ItemManagementTypeChoices => this.dataService.GetData<ItemManagementType>().AsEnumerable();

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public IEnumerable<MeasureUnit> UnitOfMeasurementChoices => this.dataService.GetData<MeasureUnit>().AsEnumerable();

        #endregion Properties

        #region Methods

        private void ExecuteHideDetailsCommand()
        {
            this.eventService.Invoke(new ShowDetailsEventArgs<Item>(false));
        }

        private void ExecuteSaveCommand()
        {
            int rowSaved = this.dataService.SaveChanges();

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<Item>(this.Item));

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
            this.Item = selectedItem;
        }

        #endregion Methods
    }
}
