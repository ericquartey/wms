using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Xpf.Data;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
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

        private ICommand deleteCommand;

        private bool itemIdHasValue;

        private InfiniteAsyncSource itemsDataSource;

        #endregion

        #region Constructors

        public CompartmentEditViewModel()
        {
            this.Title = Common.Resources.MasterData.EditCompartment;

            this.LoadData();
        }

        #endregion

        #region Properties

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(async () => await this.ExecuteDeleteCommandAsync(), this.CanExecuteDeleteCommand));

        public bool ItemIdHasValue
        {
            get => this.itemIdHasValue;
            set => this.SetProperty(ref this.itemIdHasValue, value);
        }

        public InfiniteAsyncSource ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.deleteCommand)?.RaiseCanExecuteChanged();
        }

        protected override Task ExecuteRefreshCommandAsync()
        {
            throw new NotSupportedException();
        }

        protected override Task ExecuteRevertCommand() => throw new NotSupportedException();

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit, int>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

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
                e.PropertyName == nameof(CompartmentDetails.Height)))
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

        private async Task ExecuteDeleteCommandAsync()
        {
            this.IsBusy = true;

            var userChoice = this.DialogService.ShowMessage(
                DesktopApp.AreYouSureToDeleteCompartment,
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var loadingUnit = this.Model.LoadingUnit;
                var result = await this.compartmentProvider.DeleteAsync(this.Model.Id);
                if (result.Success)
                {
                    loadingUnit.Compartments.Remove(this.Model as ICompartment);

                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CompartmentDeletedSuccessfully, StatusType.Success));

                    this.IsBusy = false;
                    this.Model = null;
                    this.CompleteOperation();
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        private void LoadData()
        {
            this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(this.itemProvider).DataSource;
        }

        #endregion
    }
}
