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
    public class CompartmentDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private CompartmentDetails compartment;
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
            this.LoadData((int?)this.Data);
            base.OnAppear();
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.compartmentProvider.Save(this.Compartment);

            if (rowSaved != 0)
            {
                this.EventService.Invoke(new ItemChangedEvent<CompartmentDetails, int>(this.Compartment.Id));

                ServiceLocator.Current.GetInstance<IEventService>()
                              .Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.EventService.Subscribe<ItemSelectionChangedEvent<Compartment, int>>(
                    eventArgs => this.LoadData(eventArgs.ItemId), true);
        }

        private void LoadData(int? itemId)
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
