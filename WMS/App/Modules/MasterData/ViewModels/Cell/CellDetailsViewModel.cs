using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Modules.BLL;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellDetailsViewModel : DetailsViewModel<CellDetails>
    {
        #region Fields

        private readonly ICellProvider cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();

        private readonly ILoadingUnitProvider loadingUnitsProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private bool cellHasLoadingUnits;

        private IDataSource<LoadingUnitDetails, int> loadingUnitsDataSource;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private object selectedLoadingUnit;

        #endregion

        #region Constructors

        public CellDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public bool CellHasLoadingUnits
        {
            get => this.cellHasLoadingUnits;
            set => this.SetProperty(ref this.cellHasLoadingUnits, value);
        }

        public LoadingUnitDetails CurrentLoadingUnit
        {
            get
            {
                if (this.selectedLoadingUnit == null)
                {
                    return default(LoadingUnitDetails);
                }

                if ((this.selectedLoadingUnit is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(LoadingUnitDetails);
                }

                return (LoadingUnitDetails)((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedLoadingUnit).OriginalRow;
            }
        }

        public IDataSource<LoadingUnitDetails, int> LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
        }

        public object SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            set
            {
                this.SetProperty(ref this.selectedLoadingUnit, value);
                this.RaisePropertyChanged(nameof(this.CurrentLoadingUnit));
            }
        }

        #endregion

        #region Methods

        public override async void LoadRelatedData()
        {
            if (!this.IsModelIdValid)
            {
                return;
            }

            var loadingUnit = await this.loadingUnitsProvider.GetByCellIdAsync(this.Model.Id);
            this.LoadingUnitsDataSource = this.Model != null
                ? new DataSource<LoadingUnitDetails, int>(() => loadingUnit.AsQueryable<LoadingUnitDetails>())
                : null;
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.cellProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Cell, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CellSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);

            await base.OnAppearAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<Cell>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<Cell, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Cell>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<Cell>>(async eventArgs => { await this.LoadDataAsync(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<Cell, int>>(async eventArgs => { await this.LoadDataAsync(); });
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<Cell>>(
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

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    this.Model = await this.cellProvider.GetByIdAsync(modelId);
                    this.CellHasLoadingUnits = this.Model.LoadingUnitsCount > 0 ? true : false;
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }
        }

        #endregion
    }
}
