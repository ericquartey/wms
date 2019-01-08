using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentEditViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private ICommand deleteCommand;

        private bool itemIdHasValue;

        private IDataSource<Item> itemsDataSource;

        #endregion Fields

        #region Constructors

        public CompartmentEditViewModel()
        {
            this.Title = Common.Resources.MasterData.EditCompartment;
            this.ItemsDataSource = new DataSource<Item>(() => this.itemProvider.GetAll());
        }

        #endregion Constructors

        #region Properties

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(async () => await this.ExecuteDeleteCommand(), this.CanExecuteDeleteCommand));

        public bool ItemIdHasValue
        {
            get => this.itemIdHasValue;
            set => this.SetProperty(ref this.itemIdHasValue, value);
        }

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        #endregion Properties

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.deleteCommand)?.RaiseCanExecuteChanged();
        }

        protected override Task ExecuteRevertCommand()
        {
            // do nothing here
            return null;
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.compartmentProvider.SaveAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CompartmentDetails.ItemId))
            {
                this.ItemIdHasValue = this.Model.ItemId.HasValue;
            }

            if (this.Model.ItemId.HasValue
                &&
                (
                e.PropertyName == nameof(CompartmentDetails.ItemId)
                ||
                e.PropertyName == nameof(CompartmentDetails.Width)
                ||
                e.PropertyName == nameof(CompartmentDetails.Height)
                ))
            {
                var capacity = await this.compartmentProvider.GetMaxCapacityAsync(
                    this.Model.Width,
                    this.Model.Height,
                    this.Model.ItemId.Value);

                this.Model.MaxCapacity = capacity ?? this.Model.MaxCapacity;
            }

            base.Model_PropertyChanged(sender, e);
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.Model != null;
        }

        private async Task ExecuteDeleteCommand()
        {
            this.IsBusy = true;

            var result = this.DialogService.ShowMessage(
                DesktopApp.AreYouSureToDeleteCompartment,
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                var loadingUnit = this.Model.LoadingUnit;
                var affectedRowsCount = await this.compartmentProvider.DeleteAsync(this.Model.Id);
                if (affectedRowsCount > 0)
                {
                    loadingUnit.Compartments.Remove(this.Model as ICompartment);

                    this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CompartmentDeletedSuccessfully, StatusType.Success));

                    this.IsBusy = false;
                    this.Model = null;
                    this.CompleteOperation();
                }
                else
                {
                    this.EventService.Invoke(new StatusEventArgs(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        #endregion Methods
    }
}
