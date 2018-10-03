using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Modules.BLL.Services;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private IDataSource<Compartment> compartmentsDataSource;
        private ICommand hideDetailsCommand;
        private ItemDetails item;
        private bool itemHasCompartments;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<Compartment> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
                                    (this.hideDetailsCommand = new DelegateCommand(this.ExecuteHideDetailsCommand));

        public ItemDetails Item
        {
            get => this.item;
            set
            {
                if (this.SetProperty(ref this.item, value))
                {
                    if (this.item != null)
                    {
                        var viewName = MvvmNaming.GetViewNameFromViewModelName(nameof(ItemDetailsViewModel));
                        this.CompartmentsDataSource = this.dataSourceService.GetAll<Compartment>(viewName, this.item.Id).Single();
                    }
                    else
                    {
                        this.CompartmentsDataSource = null;
                    }
                }
            }
        }

        public bool ItemHasCompartments
        {
            get => this.itemHasCompartments;
            set => this.SetProperty(ref this.itemHasCompartments, value);
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
            var rowSaved = this.itemProvider.Save(this.Item);

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
                    eventArgs => this.OnItemSelectionChanged(eventArgs.ItemId), true);
        }

        private void OnItemSelectionChanged(object itemId)
        {
            if (itemId == null)
            {
                this.Item = null;
                return;
            }
            this.Item = this.itemProvider.GetById((int)itemId);

            this.ItemHasCompartments = this.itemProvider.HasAnyCompartments((int)itemId);
        }

        #endregion Methods
    }
}
