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
                    // set compartments
                }
            }
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        #endregion Properties

        #region Methods

        private void ExecuteHideDetailsCommand()
        {
            this.eventService.Invoke(new ShowDetailsEventArgs<Item>(false));
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
            this.eventService.Subscribe<ItemSelectionChangedEvent<ItemDetails>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
        }

        private void OnItemSelectionChanged(ItemDetails selectedItem)
        {
            this.Item = selectedItem;
        }

        #endregion Methods

        #region Classes

        /*public class CompartmentsDataSource : IDataSource<Compartment>
        {
            #region Constructors

            public CompartmentsDataSource(ItemDetails item, IDataService dataService)
            {
                this.Item = item;
                this.DataService = dataService;
            }

            #endregion Constructors

            #region Properties

            public Int32 Count => 0;

            public IDataService DataService { get; private set; }
            public Func<IEnumerable<Compartment>, IEnumerable<Compartment>> Filter => compartments => compartments;
            public Item Item { get; private set; }
            public String Name => "Item Compartments";

            #endregion Properties

            #region Methods

            public IEnumerable<Compartment> Load()
            {
                return this.Item != null ?
                           this.DataService.GetData<Compartment>().Where(compartment => compartment.ItemId == this.Item.Id)
                           : null;
            }

            IEnumerable<Compartment> IDataSource<Compartment>.Load()
            {
                throw new NotImplementedException();
            }

            #endregion Methods
        }*/

        #endregion Classes
    }
}
