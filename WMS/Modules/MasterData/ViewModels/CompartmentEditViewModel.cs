using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentEditViewModel : SidePanelDetailsViewModel<CompartmentEdit>
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
            (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand));

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        #endregion Properties

        #region Methods

        public async void Initialize(int compartmentId, LoadingUnitDetails loadingUnit)
        {
            await this.LoadData(compartmentId, loadingUnit);
        }

        public override void RefreshData()
        {
            // do nothing here
        }

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
            var affectedRowsCount = this.loadingUnitProvider.Save(this.Model.LoadingUnit);
            if (affectedRowsCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.Model.LoadingUnit.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));

                this.OnOperationComplete(null);
            }
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.Model != null;
        }

        private void ExecuteDeleteCommand()
        {
            var affectedRowsCount = this.compartmentProvider.Delete(this.Model.Id);
            if (affectedRowsCount > 0)
            {
                this.Model.LoadingUnit.Compartments.Remove(this.Model as ICompartment);
                this.Model = null;
                this.OnOperationComplete(null);
            }
        }

        private async Task LoadData(int compartmentId, LoadingUnitDetails loadingUnit)
        {
            this.Model = await this.compartmentProvider.GetEditableById(compartmentId);
            this.Model.LoadingUnit = loadingUnit;
        }

        #endregion Methods
    }
}
