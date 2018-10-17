using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private IDataSource<AllowedItemInCompartment> allowedItemsDataSource;
        private CompartmentDetails compartment;
        private bool compartmentHasAllowedItems;
        private object modelSelectionChangedSubscription;
        private ICommand saveCommand;
        private object selectedAllowedItem;

        #endregion Fields

        #region Constructors

        public CompartmentDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<AllowedItemInCompartment> AllowedItemsDataSource
        {
            get => this.allowedItemsDataSource;
            set => this.SetProperty(ref this.allowedItemsDataSource, value);
        }

        public CompartmentDetails Compartment
        {
            get => this.compartment;
            set
            {
                if (!this.SetProperty(ref this.compartment, value))
                {
                    return;
                }

                this.AllowedItemsDataSource = this.compartment != null
                    ? this.dataSourceService
                        .GetAll<AllowedItemInCompartment>(nameof(CompartmentDetailsViewModel), this.compartment.Id)
                        .Single()
                    : null;
            }
        }

        public bool CompartmentHasAllowedItems
        {
            get => this.compartmentHasAllowedItems;
            set => this.SetProperty(ref this.compartmentHasAllowedItems, value);
        }

        public AllowedItemInCompartment CurrentAllowedItemInCompartment
        {
            get
            {
                if (this.SelectedAllowedItem == null)
                {
                    return default(AllowedItemInCompartment);
                }
                if ((this.SelectedAllowedItem is DevExpress.Data.Async.Helpers
                        .ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(AllowedItemInCompartment);
                }
                return (AllowedItemInCompartment)
                    (((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this
                        .SelectedAllowedItem).OriginalRow);
            }
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public object SelectedAllowedItem
        {
            get => this.selectedAllowedItem;
            set
            {
                this.SetProperty(ref this.selectedAllowedItem, value);
                this.RaisePropertyChanged(nameof(this.CurrentAllowedItemInCompartment));
            }
        }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            if (this.Data is int modelId)
            {
                this.LoadData(modelId);
            }

            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Compartment, int>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.compartmentProvider.Save(this.Compartment);

            if (modifiedRowCount > 0)
            {
                this.Compartment = this.compartmentProvider.GetById(this.Compartment.Id);

                this.EventService.Invoke(new ModelChangedEvent<CompartmentDetails, int>(this.Compartment.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Compartment, int>>(
                eventArgs =>
                {
                    if (eventArgs.ModelIdHasValue)
                    {
                        this.LoadData(eventArgs.ModelId);
                    }
                    else
                    {
                        this.Compartment = null;
                    }
                },
                true);
        }

        private void LoadData(int modelId)
        {
            this.Compartment = this.compartmentProvider.GetById(modelId);

            this.CompartmentHasAllowedItems = this.compartmentProvider.HasAnyAllowedItem(modelId);
        }

        #endregion Methods
    }
}
