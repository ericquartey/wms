using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellDetailsViewModel : DetailsViewModel<CellDetails>, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICellProvider cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
        private readonly ILoadingUnitProvider loadingUnitsProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private CellDetails cell;
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

        public CellDetails Cell
        {
            get => this.cell;
            set
            {
                if (!this.SetProperty(ref this.cell, value))
                {
                    return;
                }

                this.ChangeDetector.TakeSnapshot(this.cell);

                this.RefreshData();
            }
        }

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

        public void RefreshData()
        {
            this.LoadingUnitsDataSource = this.cell != null
                ? new DataSource<LoadingUnitDetails>(() => this.loadingUnitsProvider.GetByCellId(this.cell.Id))
                : null;
        }

        protected override void ExecuteRevertCommand()
        {
            this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.cellProvider.Save(this.cell);
            if (modifiedRowCount > 0)
            {
                this.ChangeDetector.TakeSnapshot(this.cell);

                this.EventService.Invoke(new ModelChangedEvent<Cell>(this.cell.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CellSavedSuccessfully));
            }
        }

        protected override void OnAppear()
        {
            this.LoadData();
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
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Cell>>(eventArgs => { this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Cell>>(eventArgs => { this.LoadData(); });
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Cell>>(
                eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        this.LoadData();
                    }
                    else
                    {
                        this.Cell = null;
                    }
                },
                this.Token,
                true,
                true);
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Cell = this.cellProvider.GetById(modelId);
                this.CellHasLoadingUnits = this.cellProvider.HasAnyLoadingUnits(modelId);
            }
        }

        #endregion Methods
    }
}
