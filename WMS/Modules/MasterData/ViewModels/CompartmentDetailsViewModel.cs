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
    public class CompartmentDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private CompartmentDetails compartment;
        private ICommand hideDetailsCommand;
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

        public ICommand HideDetailsCommand => this.hideDetailsCommand ??
                    (this.hideDetailsCommand = new DelegateCommand(this.ExecuteHideDetailsCommand));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        #endregion Properties

        #region Methods

        private void ExecuteHideDetailsCommand()
        {
            this.eventService.Invoke(new ShowDetailsEventArgs<Compartment>(this.Token, false));
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.compartmentProvider.Save(this.Compartment);

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<CompartmentDetails>(this.Compartment));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.eventService.Subscribe<ItemSelectionChangedEvent<Compartment>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.ItemId), true);
        }

        private void OnItemSelectionChanged(object itemId)
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
