using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private CompartmentDetails compartment;
        private object itemSelectionChangedSubscription;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public CompartmentDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails Compartment
        {
            get => this.compartment;
            set => this.SetProperty(ref this.compartment, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

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
            this.EventService.Unsubscribe<ItemSelectionChangedEvent<Compartment, int>>(this.itemSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.compartmentProvider.Save(this.Compartment);

            if (modifiedRowCount > 0)
            {
                this.Compartment = this.compartmentProvider.GetById(this.Compartment.Id);

                this.EventService.Invoke(new ItemChangedEvent<CompartmentDetails, int>(this.Compartment.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.itemSelectionChangedSubscription = this.EventService.Subscribe<ItemSelectionChangedEvent<Compartment, int>>(
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
        }

        #endregion Methods
    }
}
