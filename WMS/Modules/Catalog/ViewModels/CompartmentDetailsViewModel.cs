using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
    public class CompartmentDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();
        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private Compartment compartment;
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

        public IEnumerable<AbcClass> AbcClassChoices => this.dataService.GetData<AbcClass>().AsEnumerable();

        public Compartment Compartment
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
            this.eventService.Invoke(new ShowDetailsEventArgs<Compartment>(false));
        }

        private void ExecuteSaveCommand()
        {
            int rowSaved = this.dataService.SaveChanges();

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<Compartment>(this.Compartment));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEvent(Ferretto.Common.Resources.Catalog.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.eventService.Subscribe<ItemSelectionChangedEvent<Compartment>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
        }

        private void OnItemSelectionChanged(Compartment selectedCompartment)
        {
            this.Compartment = selectedCompartment;
        }

        #endregion Methods
    }
}
