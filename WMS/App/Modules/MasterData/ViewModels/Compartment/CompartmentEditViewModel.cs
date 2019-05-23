using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentEditViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand deleteCompartmentCommand;

        private bool itemIdHasValue;

        private InfiniteAsyncSource itemsDataSource;

        #endregion

        #region Constructors

        public CompartmentEditViewModel()
        {
            this.Title = Common.Resources.MasterData.EditCompartment;
            this.LoadDataAsync();
        }

        #endregion

        #region Properties

        public ICommand DeleteCompartmentCommand => this.deleteCompartmentCommand ??
            (this.deleteCompartmentCommand = new DelegateCommand(
                async () => await this.DeleteCompartmentAsync(),
                this.CanDeleteCompartment));

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

            ((DelegateCommand)this.deleteCompartmentCommand)?.RaiseCanExecuteChanged();
        }

        protected override Task ExecuteRefreshCommandAsync()
        {
            throw new NotSupportedException();
        }

        protected override Task ExecuteRevertCommandAsync() => throw new NotSupportedException();

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            this.IsBusy = true;

            var result = await this.compartmentProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override Task LoadDataAsync()
        {
            Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<Item>>> getAllAllowedByLoadingUnitId = this.GetAllAllowedByLoadingUnitIdAsync;
            this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(
            this.itemProvider, getAllAllowedByLoadingUnitId).DataSource;

            return Task.CompletedTask;
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
                var result = await this.compartmentProvider.GetMaxCapacityAsync(
                    this.Model.Width,
                    this.Model.Height,
                    this.Model.ItemId.Value);

                if (result.Success)
                {
                    this.Model.MaxCapacity = result.Entity;
                }
            }

            base.Model_PropertyChanged(sender, e);
        }

        private bool CanDeleteCompartment()
        {
            return this.Model != null;
        }

        private async Task DeleteCompartmentAsync()
        {
            if (this.Model.CanDelete())
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
                        loadingUnit.Compartments.Remove(this.Model as IDrawableCompartment);

                        this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CompartmentDeletedSuccessfully, StatusType.Success));

                        this.IsBusy = false;
                        this.CompleteOperation();

                        this.Model = null;
                    }
                    else
                    {
                        this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                    }
                }

                this.IsBusy = false;
            }
            else
            {
                this.ShowErrorDialog(this.Model.GetCanDeleteReason());
            }
        }

        private async Task<IEnumerable<Item>> GetAllAllowedByLoadingUnitIdAsync(int skip, int pageSize, IEnumerable<SortOption> sortOrder)
        {
            return await this.itemProvider.GetAllAllowedByLoadingUnitIdAsync(this.Model.LoadingUnitId.Value, skip, pageSize, sortOrder);
        }

        #endregion
    }
}
