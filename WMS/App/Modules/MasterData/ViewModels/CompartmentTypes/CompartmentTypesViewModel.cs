using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.CompartmentType), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemCompartmentType), false)]
    public class CompartmentTypesViewModel : EntityPagedListViewModel<CompartmentType, int>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider = ServiceLocator.Current.GetInstance<ICompartmentTypeProvider>();

        private bool isAddShown;

        private CompartmentType newCompartmentType;

        private ICommand openCreateNewCompartmentTypeCommand;

        #endregion

        #region Constructors

        public CompartmentTypesViewModel(IDataSourceService dataSourceService)
            : base(dataSourceService)
        {
        }

        #endregion

        #region Properties

        public bool IsAddShown { get => this.isAddShown; set => this.SetProperty(ref this.isAddShown, value); }

        public CompartmentType NewCompartmentType { get => this.newCompartmentType; set => this.SetProperty(ref this.newCompartmentType, value); }

        public ICommand OpenCreateNewCompartmentTypeCommand => this.openCreateNewCompartmentTypeCommand ??
                 (this.openCreateNewCompartmentTypeCommand = new DelegateCommand(
                 this.OpenCreateNewCompartmentType));

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.COMPARTMENTTYPEDETAILS,
                this.CurrentItem.Id);
        }

        protected static bool CheckValidModel(BusinessObject model)
        {
            if (model == null)
            {
                return false;
            }

            model.IsValidationEnabled = true;

            return string.IsNullOrWhiteSpace(model.Error);
        }

        protected override async void ExecuteAddCommand()
        {
            if (CheckValidModel(this.NewCompartmentType))
            {
                await this.ExecuteAddCompartmentTypeAsync();
            }
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.compartmentTypeProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.AssociationCompartmentTypeDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private async Task<bool> ExecuteAddCompartmentTypeAsync()
        {
            this.IsBusy = true;
            var resultCreate = await this.compartmentTypeProvider.CreateAsync(this.NewCompartmentType);

            if (resultCreate.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.AssociationCompartmentTypeCreatedSuccessfully, StatusType.Success));
                this.IsAddShown = false;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(resultCreate.Description, StatusType.Error));
            }

            this.IsBusy = false;

            return resultCreate.Success;
        }

        private void OpenCreateNewCompartmentType()
        {
            this.NewCompartmentType = new CompartmentType();
        }

        #endregion
    }
}
