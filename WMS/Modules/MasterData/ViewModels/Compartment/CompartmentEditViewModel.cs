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

        protected override void ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var affectedRowsCount = this.compartmentProvider.Save(this.Model);
            if (affectedRowsCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusEventArgs(DesktopApp.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
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
                    this.EventService.Invoke(new StatusEventArgs(DesktopApp.UnableToSaveChanges, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        #endregion Methods
    }
}
