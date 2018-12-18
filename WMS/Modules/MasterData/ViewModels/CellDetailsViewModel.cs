using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellDetailsViewModel : DetailsViewModel<CellDetails>
    {
        #region Fields

        private readonly ICellProvider cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
        private readonly ILoadingUnitProvider loadingUnitsProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private bool cellHasLoadingUnits;
        private IDataSource<LoadingUnitDetails> loadingUnitsDataSource;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object modelSelectionChangedSubscription;
        private object selectedLoadingUnit;

        #endregion Fields

        #region Constructors

        public CellDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

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
                return (LoadingUnitDetails)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedLoadingUnit).OriginalRow);
            }
        }

        public IDataSource<LoadingUnitDetails> LoadingUnitsDataSource
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

        #endregion Properties

        #region Methods

        public override void RefreshData()
        {
            this.LoadingUnitsDataSource = this.Model != null
                ? new DataSource<LoadingUnitDetails>(() => this.loadingUnitsProvider.GetByCellId(this.Model.Id))
                : null;
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.cellProvider.Save(this.Model);
            if (modifiedRowCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<Cell>(this.Model.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CellSavedSuccessfully));
            }
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<Cell>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<Cell>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Cell>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Cell>>(async eventArgs => { await this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Cell>>(async eventArgs => { await this.LoadData(); });
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Cell>>(
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

        private async Task LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Model = await this.cellProvider.GetById(modelId);
                this.CellHasLoadingUnits = this.cellProvider.HasAnyLoadingUnits(modelId);
            }
        }

        #endregion Methods
    }
}
