using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemAddDialogViewModel : DetailsViewModel<ItemDetails>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand closeDialogCommand;

        private IDataSource<Compartment> compartmentsDataSource;

        private bool itemHasCompartments;

        private object selectedCompartment;

        #endregion

        #region Constructors

        public ItemAddDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
                                           (this.closeDialogCommand = new Prism.Commands.DelegateCommand(
                               this.ExecuteCloseDialogCommand));

        public IDataSource<Compartment> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public Compartment CurrentCompartment
        {
            get
            {
                if (this.selectedCompartment == null)
                {
                    return default(Compartment);
                }

                if ((this.selectedCompartment is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(Compartment);
                }

                return (Compartment)((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedCompartment).OriginalRow;
            }
        }

        public bool ItemHasCompartments
        {
            get => this.itemHasCompartments;
            set => this.SetProperty(ref this.itemHasCompartments, value);
        }

        public object SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.SetProperty(ref this.selectedCompartment, value);
                this.RaisePropertyChanged(nameof(this.CurrentCompartment));
            }
        }

        #endregion

        #region Methods

        protected override Task ExecuteRefreshCommandAsync()
        {
            throw new System.NotSupportedException();
        }

        protected override Task ExecuteRevertCommand()
        {
            throw new System.NotSupportedException();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.itemProvider.SaveAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        private void ExecuteCloseDialogCommand()
        {
            this.Disappear();
        }

        private void Initialize()
        {
            this.Data = null;
        }

        #endregion
    }
}
