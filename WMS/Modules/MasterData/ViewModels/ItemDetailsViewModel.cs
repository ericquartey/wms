using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private IDataSource<Compartment> compartmentsDataSource;
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

        protected override void OnAppear()
        {
            this.LoadData(this.Data);
            base.OnAppear();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemProvider.Save(this.Item);

            if (modifiedRowCount > 0)
            {
                this.Item = this.itemProvider.GetById(this.Item.Id);
                this.EventService.Invoke(new ItemChangedEvent<ItemDetails>(this.Item));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.EventService.Subscribe<ItemSelectionChangedEvent<Item>>(
                    eventArgs => this.LoadData(eventArgs.ItemId), true);
        }

        private void LoadData(object itemId)
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
