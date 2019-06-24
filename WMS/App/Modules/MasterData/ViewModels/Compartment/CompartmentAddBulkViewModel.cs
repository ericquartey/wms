using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddBulkViewModel : SidePanelDetailsViewModel<BulkCompartment>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IGlobalSettingsProvider globalSettingsProvider = ServiceLocator.Current.GetInstance<IGlobalSettingsProvider>();

        private GlobalSettings globalSettings;

        #endregion

        #region Constructors

        public CompartmentAddBulkViewModel()
        {
            this.Title = App.Resources.MasterData.BulkAddCompartment;
            this.ColorRequired = ColorRequired.CreateMode;
        }

        #endregion

        #region Properties

        public GlobalSettings GlobalSettings { get => this.globalSettings; set => this.SetProperty(ref this.globalSettings, value); }

        #endregion

        #region Methods

        public async Task InitializeDataAsync()
        {
            this.GlobalSettings = await this.globalSettingsProvider.GetAllAsync();
        }

        protected override bool CheckValidModel()
        {
            this.Model.ApplyCorrectionOnSingleCompartment(this.GlobalSettings.MinStepCompartment);
            return base.CheckValidModel();
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

            var newCompartments = this.Model.CreateBulk();
            var result = await this.compartmentProvider.AddRangeAsync(newCompartments);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(
                    App.Resources.MasterData.LoadingUnitSavedSuccessfully,
                    StatusType.Success));

                this.CompleteOperation();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
