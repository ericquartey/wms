using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : DetailsViewModel<LoadingUnitDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private IEnumerable<CompartmentDetails> compartmentsDataSource;

        private ICommand editCommand;

        private bool isCompartmentSelectableTray;

        private bool loadingUnitHasCompartments;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private bool readOnlyTray;

        private CompartmentDetails selectedCompartment;

        private Tray tray;

        private Func<ICompartment, ICompartment, string> trayColoringFunc;

        private ICommand withdrawCommand;

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

        public Func<ICompartment, ICompartment, string> TrayColoringFunc
        {
            get => this.trayColoringFunc;
            set => this.SetProperty(ref this.trayColoringFunc, value);
        }

        public ICommand WithdrawCommand => this.withdrawCommand ??
            (this.withdrawCommand = new DelegateCommand(this.ExecuteWithdrawCommand));

        #endregion Properties

        #region Methods

        public override void RefreshData()
        {
            this.CompartmentsDataSource = this.Model != null
                ? this.compartmentProvider.GetByLoadingUnitId(this.Model.Id).ToList()
                : null;
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.loadingUnitProvider.SaveAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<LoadingUnit>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<LoadingUnit>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<LoadingUnit>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteEditCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITEDIT, this.Model.Id);
        }

        private void ExecuteWithdrawCommand()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<LoadingUnit>>(
                async eventArgs => { await this.LoadData(); }, this.Token, true, true);

            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<LoadingUnit>>(
                async eventArgs => { await this.LoadData(); });

            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<LoadingUnit>>(
                async eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        await this.LoadData();
                    }
                    else
                    {
                        this.Model = null;
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
                    Height = this.Model.Length,
                    Width = this.Model.Width
                }
            };

            if (this.Model.Compartments != null)
            {
                newTray.AddCompartmentsRange(this.Model.Compartments);
            }

            this.Tray = newTray;
            this.ReadOnlyTray = true;
            this.IsCompartmentSelectableTray = true;
            this.TrayColoringFunc = (new FillingFilter()).ColorFunc;
        }

        private async Task LoadData()
        {
            this.IsBusy = true;

            if (this.Data is int modelId)
            {
                this.Model = await this.loadingUnitProvider.GetById(modelId);
                this.LoadingUnitHasCompartments = this.loadingUnitProvider.HasAnyCompartments(modelId);
                this.InitializeTray();
            }

            this.IsBusy = false;
        }

        #endregion Methods
    }
}
