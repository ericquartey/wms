using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;

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
            this.LoadData(this.Data);
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ItemSelectionChangedEvent<Compartment>>(this.itemSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.compartmentProvider.Save(this.Compartment);

            if (rowSaved != 0)
            {
                this.EventService.Invoke(new ItemChangedEvent<CompartmentDetails>(this.Compartment));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.itemSelectionChangedSubscription = this.EventService.Subscribe<ItemSelectionChangedEvent<Compartment>>(
                                        eventArgs => this.LoadData(eventArgs.ItemId), true);
        }

        private void LoadData(object itemId)
        {
            if (itemId == null)
            {
                this.Compartment = null;
                return;
            }
            this.Compartment = this.compartmentProvider.GetById((int)itemId);
        }

        #endregion Methods
    }
}
