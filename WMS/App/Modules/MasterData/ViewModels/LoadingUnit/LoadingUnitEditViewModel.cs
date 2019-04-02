using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitEditViewModel : BaseServiceNavigationViewModel, IExtensionDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly Func<ICompartment, ICompartment, string> filterColorFunc = new EditFilter().ColorFunc;

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private BaseNavigationViewModel activeSideViewModel;

        private ICommand addCompartmentCommand;

        private ICommand bulkAddCompartmentCommand;

        private IEnumerable<CompartmentDetails> compartmentsDataSource;

        private ICommand editCompartmentCommand;

        private bool isSidePanelOpen;

        private LoadingUnitDetails loadingUnit;

        private bool loadingUnitHasCompartments;

        private ICompartment selectedCompartmentTray;

        #endregion

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.loadingUnit = new LoadingUnitDetails();
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
                this.BulkAddCompartment));

        public ColorRequired ColorRequired => ColorRequired.Default;

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

        public Func<ICompartment, ICompartment, string> FilterColorFunc => this.filterColorFunc;

        public bool IsSidePanelOpen
        {
            get => this.isSidePanelOpen;
            set => this.SetProperty(ref this.isSidePanelOpen, value);
        }

        public LoadingUnitDetails LoadingUnitDetails => this.loadingUnit;

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public ICompartment SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        #endregion

        #region Methods

        public async void LoadRelatedData()
        {
            this.CompartmentsDataSource = (this.loadingUnit != null && this.loadingUnit.Id != 0)
                ? await this.compartmentProvider.GetByLoadingUnitIdAsync(this.loadingUnit.Id)
                : null;
        }

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);
            await base.OnAppearAsync().ConfigureAwait(true);
            this.LoadRelatedData();
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

                    case ICompartment compartment:
                        this.SelectedCompartmentTray = compartment;
                        break;
                }
            }
        }

        private bool CanEditCommand()
        {
            return this.selectedCompartmentTray != null;
        }

        private async Task AddCompartmentAsync()
        {
            this.SelectedCompartmentTray = null;

            var model = await this.compartmentProvider.GetNewAsync();
            model.LoadingUnitId = this.loadingUnit.Id;
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentAddViewModel { Model = model });
        }

        private void BulkAddCompartment()
        {
            this.SelectedCompartmentTray = null;

            var model = new BulkCompartment();
            model.LoadingUnitId = this.loadingUnit.Id;
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentAddBulkViewModel { Model = model });
        }

        private async Task EditCompartmentAsync()
        {
            var model = await this.compartmentProvider.GetByIdAsync(this.selectedCompartmentTray.Id);
            model.LoadingUnit = this.loadingUnit;

            this.ShowSidePanel(new CompartmentEditViewModel { Model = model });
        }

        private async Task ExtractArgsInputAsync(LoadingUnitArgs input)
        {
            if (input != null)
            {
                this.loadingUnit = await this.loadingUnitProvider.GetByIdAsync(input.LoadingUnitId);
                if (input.CompartmentId.HasValue)
                {
                    this.SelectedCompartmentTray = await this.compartmentProvider.GetByIdAsync(input.CompartmentId.Value);
                }
            }
        }

        private void HideSidePanel()
        {
            this.IsSidePanelOpen = false;
            this.ActiveSideViewModel = null;
        }

        private async Task LoadDataAsync()
        {
            if (this.Data is int modelId)
            {
                this.loadingUnit = await this.loadingUnitProvider.GetByIdAsync(modelId);
            }
            else if (this.Data is LoadingUnitArgs input)
            {
                await this.ExtractArgsInputAsync(input);
            }

            this.RaisePropertyChanged(nameof(this.LoadingUnitDetails));
        }

        private void ShowSidePanel(BaseNavigationViewModel childViewModel)
        {
            this.ActiveSideViewModel = childViewModel;
            this.IsSidePanelOpen = true;
        }

        #endregion
    }
}
