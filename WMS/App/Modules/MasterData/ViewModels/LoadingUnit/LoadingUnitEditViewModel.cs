using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Cell), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemCompartmentType), false)]
    public class LoadingUnitEditViewModel : DetailsViewModel<LoadingUnitDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc = new EditFilter().ColorFunc;

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private BaseNavigationViewModel activeSideViewModel;

        private ICommand addCompartmentCommand;

        private ICommand bulkAddCompartmentCommand;

        private IEnumerable<CompartmentDetails> compartmentsDataSource;

        private ICommand editCompartmentCommand;

        private bool isSidePanelOpen;

        private bool loadingUnitHasCompartments;

        private IDrawableCompartment selectedCompartmentTray;

        private string subTitle;

        #endregion

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.HideSidePanel();
        }

        #endregion

        #region Properties

        public BaseNavigationViewModel ActiveSideViewModel
        {
            get => this.activeSideViewModel;
            set
            {
                var prevValue = this.activeSideViewModel;
                if (this.SetProperty(ref this.activeSideViewModel, value))
                {
                    if (prevValue != null)
                    {
                        (prevValue as ICompletable).OperationComplete -= this.ActiveSideViewModel_OperationComplete;
                    }

                    if (value != null)
                    {
                        (value as ICompletable).OperationComplete += this.ActiveSideViewModel_OperationComplete;
                    }
                }
            }
        }

        public ICommand AddCompartmentCommand => this.addCompartmentCommand ??
            (this.addCompartmentCommand = new DelegateCommand(
                async () => await this.AddCompartmentAsync()));

        public ICommand BulkAddCompartmentCommand => this.bulkAddCompartmentCommand ??
            (this.bulkAddCompartmentCommand = new DelegateCommand(
                async () => await this.BulkAddCompartmentAsync(),
                this.CanBulkAddCommand));

        public IEnumerable<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand EditCompartmentCommand => this.editCompartmentCommand ??
            (this.editCompartmentCommand = new DelegateCommand(
                    async () => await this.EditCompartmentAsync(),
                    this.CanEditCommand)
            .ObservesProperty(() => this.SelectedCompartmentTray));

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc => this.filterColorFunc;

        public bool IsSidePanelOpen
        {
            get => this.isSidePanelOpen;
            set => this.SetProperty(ref this.isSidePanelOpen, value);
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public IDrawableCompartment SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        public string SubTitle
        {
            get => this.subTitle;
            set => this.SetProperty(ref this.subTitle, value);
        }

        #endregion

        #region Methods

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override Task ExecuteRevertCommandAsync()
        {
            throw new NotSupportedException();
        }

        protected override async Task LoadDataAsync()
        {
            if (this.Data is LoadingUnitEditViewData inputData)
            {
                await this.ExtractInputDataAsync(inputData);
            }
        }

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);
            await base.OnAppearAsync().ConfigureAwait(true);
        }

        private async void ActiveSideViewModel_OperationComplete(object sender, OperationEventArgs e)
        {
            this.HideSidePanel();

            if (e.IsCanceled == false)
            {
                this.SelectedCompartmentTray = null;
                await this.LoadDataAsync();

                switch (e.Model)
                {
                    case BulkCompartment bulk:
                        this.SelectedCompartmentTray = bulk.LoadingUnit.Compartments.FirstOrDefault();
                        break;

                    case IDrawableCompartment compartment:
                        this.SelectedCompartmentTray = compartment;
                        break;
                }
            }
        }

        private async Task AddCompartmentAsync()
        {
            this.SelectedCompartmentTray = null;

            var result = await this.compartmentProvider.GetNewAsync();
            if (!result.Success)
            {
                return;
            }

            var model = result.Entity;
            model.LoadingUnitId = this.Model.Id;
            model.LoadingUnit = this.Model;

            var viewModel = new CompartmentEditViewModel { Model = model, Mode = CompartmentEditViewModel.AppearMode.Add };
            if (this.Data is LoadingUnitEditViewData inputData)
            {
                model.ItemId = inputData.ItemId;
                await viewModel.InitializeDataAsync();
                this.ShowSidePanel(viewModel);
            }
        }

        private async Task BulkAddCompartmentAsync()
        {
            this.SelectedCompartmentTray = null;

            var model = new BulkCompartment
            {
                LoadingUnitId = this.Model.Id,
                LoadingUnit = this.Model
            };

            var viewModel = new CompartmentAddBulkViewModel { Model = model };
            await viewModel.InitializeDataAsync();
            this.ShowSidePanel(viewModel);
        }

        private bool CanBulkAddCommand()
        {
            return this.Data is LoadingUnitEditViewData inputData
                && inputData.ItemId.HasValue == false;
        }

        private bool CanEditCommand()
        {
            return this.selectedCompartmentTray != null
                && this.Data is LoadingUnitEditViewData inputData
                && inputData.ItemId.HasValue == false;
        }

        private async Task EditCompartmentAsync()
        {
            var model = await this.compartmentProvider.GetByIdAsync(this.selectedCompartmentTray.Id);
            model.LoadingUnit = this.Model;
            var viewModel = new CompartmentEditViewModel { Model = model, Mode = CompartmentEditViewModel.AppearMode.Edit };
            await viewModel.InitializeDataAsync();
            this.ShowSidePanel(viewModel);
        }

        private async Task ExtractInputDataAsync(LoadingUnitEditViewData inputData)
        {
            System.Diagnostics.Debug.Assert(inputData != null, "inputData should never be null");

            this.IsBusy = true;

            this.Model = await this.loadingUnitProvider.GetByIdAsync(inputData.LoadingUnitId);
            if (inputData.SelectedCompartmentId.HasValue && this.Model.Compartments.Any(c => c.Id == inputData.SelectedCompartmentId))
            {
                this.SelectedCompartmentTray = await this.compartmentProvider.GetByIdAsync(inputData.SelectedCompartmentId.Value);
            }
            else
            {
                this.SelectedCompartmentTray = null;
                inputData.SelectedCompartmentId = null;
            }

            if (inputData.ItemId.HasValue)
            {
                var item = await this.itemProvider.GetByIdAsync(inputData.ItemId.Value);
                this.SubTitle = string.Format(Common.Resources.MasterData.LoadingUnitEditForItemSubTitle, this.Model.Code, item.Code);
            }
            else
            {
                this.SubTitle = string.Format(Common.Resources.MasterData.LoadingUnitEditSubTitle, this.Model.Code);
            }

            await this.LoadCompartmentsDataSourceAsync();

            this.IsBusy = false;
        }

        private void HideSidePanel()
        {
            this.IsSidePanelOpen = false;
            this.ActiveSideViewModel = null;
        }

        private async Task LoadCompartmentsDataSourceAsync()
        {
            if (this.Model == null)
            {
                this.CompartmentsDataSource = null;
            }
            else
            {
                var result = await this.compartmentProvider.GetByLoadingUnitIdAsync(this.Model.Id).ConfigureAwait(true);

                this.CompartmentsDataSource = result.Success ? result.Entity : null;
            }
        }

        private void ShowSidePanel(BaseNavigationViewModel childViewModel)
        {
            this.ActiveSideViewModel = childViewModel;
            this.IsSidePanelOpen = true;
        }

        #endregion
    }
}
