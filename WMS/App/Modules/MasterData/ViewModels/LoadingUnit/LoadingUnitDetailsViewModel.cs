using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : DetailsViewModel<LoadingUnitDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private IEnumerable<CompartmentDetails> compartmentsDataSource;

        private ICommand editLoadingUnitCommand;

        private bool isCompartmentSelectableTray;

        private LoadingUnitDetails loadingUnit;

        private bool loadingUnitHasCompartments;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private bool readOnlyTray;

        private CompartmentDetails selectedCompartment;

        private Func<IDrawableCompartment, IDrawableCompartment, string> trayColoringFunc;

        private ICommand withdrawLoadingUnitCommand;

        private string withdrawReason;

        #endregion

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public IEnumerable<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand EditLoadingUnitCommand => this.editLoadingUnitCommand ??
               (this.editLoadingUnitCommand = new DelegateCommand(this.EditLoadingUnit));

        public bool IsCompartmentSelectableTray
        {
            get => this.isCompartmentSelectableTray;
            set => this.SetProperty(ref this.isCompartmentSelectableTray, value);
        }

        public LoadingUnitDetails LoadingUnitDetails => this.loadingUnit;

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

        public Func<IDrawableCompartment, IDrawableCompartment, string> TrayColoringFunc
        {
            get => this.trayColoringFunc;
            set => this.SetProperty(ref this.trayColoringFunc, value);
        }

        public ICommand WithdrawLoadingUnitCommand => this.withdrawLoadingUnitCommand ??
            (this.withdrawLoadingUnitCommand = new DelegateCommand(WithdrawLoadingUnit));

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override async void LoadRelatedData()
        {
            this.CompartmentsDataSource = this.Model != null
                ? await this.compartmentProvider.GetByLoadingUnitIdAsync(this.Model.Id)
                : null;
        }

        public override void UpdateMoreReasons()
        {
            this.WithdrawReason = this.Model?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            this.IsBusy = true;

            var result = await this.loadingUnitProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);
            await base.OnAppearAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<LoadingUnit>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<LoadingUnit, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<LoadingUnit>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private static void WithdrawLoadingUnit()
        {
            throw new NotImplementedException();
        }

        private void EditLoadingUnit()
        {
            var args = new LoadingUnitArgs { LoadingUnitId = this.Model.Id, CompartmentId = this.SelectedCompartment?.Id };
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITEDIT, args);
        }

        private void Initialize()
        {
            this.loadingUnit = new LoadingUnitDetails();
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<LoadingUnit>>(
                async eventArgs => { await this.LoadDataAsync(); }, this.Token, true, true);

            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<LoadingUnit, int>>(
                async eventArgs => { await this.LoadDataAsync(); });

            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<LoadingUnit>>(
                async eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        await this.LoadDataAsync();
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
            this.loadingUnit = this.Model;
            this.IsCompartmentSelectableTray = true;
            this.TrayColoringFunc = new FillingFilter().ColorFunc;
            this.RaisePropertyChanged(nameof(this.LoadingUnitDetails));
        }

        private async Task LoadDataAsync()
        {
            if (this.Data is int modelId)
            {
                try
                {
                    this.IsBusy = true;

                    this.Model = await this.loadingUnitProvider.GetByIdAsync(modelId);
                    this.LoadingUnitHasCompartments = this.Model.CompartmentsCount > 0 ? true : false;
                    this.InitializeTray();
                    this.IsBusy = false;
                }
                catch
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
                }
            }
        }

        #endregion
    }
}
