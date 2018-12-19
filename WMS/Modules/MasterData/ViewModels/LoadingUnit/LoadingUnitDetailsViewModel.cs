using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : DetailsViewModel<LoadingUnitDetails>, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private IEnumerable<CompartmentDetails> compartmentsDataSource;
        private ICommand editCommand;
        private bool isCompartmentSelectableTray;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object modelSelectionChangedSubscription;
        private bool readOnlyTray;

        private CompartmentDetails selectedCompartment;
        private Tray tray;
        private Func<CompartmentDetails, CompartmentDetails, string> trayColoringFunc;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand EditCommand => this.editCommand ??
               (this.editCommand = new DelegateCommand(this.ExecuteEditCommand));

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set
            {
                if (!this.SetProperty(ref this.loadingUnit, value))
                {
                    return;
                }

                this.TakeSnapshot(this.loadingUnit);

                this.RefreshData();
            }
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        public CompartmentDetails SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        public Func<CompartmentDetails, CompartmentDetails, string> TrayColoringFunc
        {
            get => this.trayColoringFunc;
            set => this.SetProperty(ref this.trayColoringFunc, value);
        }

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = this.loadingUnit != null
                ? this.compartmentProvider.GetByLoadingUnitId(this.loadingUnit.Id).ToList()
                : null;
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.loadingUnitProvider.Save(this.LoadingUnit);

            if (modifiedRowCount <= 0)
            {
                return;
            }

            this.TakeSnapshot(this.loadingUnit);

            this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.loadingUnit.Id));

            this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<LoadingUnit>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<LoadingUnit>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<LoadingUnit>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteEditCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITEDIT, this.LoadingUnit.Id);
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<LoadingUnit>>(async eventArgs => { await this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<LoadingUnit>>(async eventArgs => { await this.LoadData(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedEvent<LoadingUnit>>(
                    async eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            await this.LoadData();
                        }
                        else
                        {
                            this.LoadingUnit = null;
                        }
                    },
                    this.Token,
                    true,
                    true);
        }

        private void InitializeTray()
        {
            var newTray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = this.LoadingUnit.Length,
                    Width = this.LoadingUnit.Width
                }
            };

            if (this.LoadingUnit.Compartments != null)
            {
                newTray.AddCompartmentsRange(this.LoadingUnit.Compartments);
            }

            this.Tray = newTray;
            this.ReadOnlyTray = true;
            this.IsCompartmentSelectableTray = true;
            this.TrayColoringFunc = (new FillingFilter()).ColorFunc;
        }

        private async Task LoadData()
        {
            if (!(this.Data is int modelId))
            {
                return;
            }

            this.LoadingUnit = await this.loadingUnitProvider.GetById(modelId);
            this.LoadingUnitHasCompartments = this.loadingUnitProvider.HasAnyCompartments(modelId);

            this.InitializeTray();
        }

        #endregion Methods
    }
}
