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
    public class CompartmentDetailsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IBusinessProvider businessProvider = ServiceLocator.Current.GetInstance<IBusinessProvider>();
        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();
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
            this.eventService.Invoke(new ShowDetailsEventArgs<Compartment>(false));
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.businessProvider.Save(this.Compartment);

            if (rowSaved != 0)
            {
                this.eventService.Invoke(new ItemChangedEvent<CompartmentDetails>(this.Compartment));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEvent(Ferretto.Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.eventService.Subscribe<ItemSelectionChangedEvent<CompartmentDetails>>(
                    eventArgs => this.OnItemSelectionChanged(eventArgs.SelectedItem), true);
        }

        private void OnItemSelectionChanged(CompartmentDetails selectedCompartment)
        {
            this.Compartment = selectedCompartment;
        }

        #endregion Methods
    }
}
