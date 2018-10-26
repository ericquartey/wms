using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellDetailsViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly ICellProvider cellProvider = ServiceLocator.Current.GetInstance<ICellProvider>();
        private IDataSource<LoadingUnitDetails> loadingUnitsDataSource;
        private CellDetails cell;
        private bool cellHasLoadingUnits;
        private object modelSelectionChangedSubscription;
        private ICommand revertCommand;
        private ICommand saveCommand;
        private object selectedLoadingUnit;

        #endregion Fields

        #region Constructors

        public CellDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<LoadingUnitDetails> LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
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

        public CellDetails Cell
        {
            get => this.cell;
            set
            {
                if (!this.SetProperty(ref this.cell, value))
                {
                    return;
                }
                this.RefreshData();
             }
        }

        public bool CellHasLoadingUnits
        {
            get => this.cellHasLoadingUnits;
            set => this.SetProperty(ref this.cellHasLoadingUnits, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
          (this.revertCommand = new DelegateCommand(this.LoadData));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

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

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Cell>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.cellProvider.Save(this.Cell);

            if (modifiedRowCount > 0)
            {
                this.EventService.Invoke(new ModelChangedEvent<Cell>(this.Cell.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.CellSavedSuccessfully));
            }
        }

        private void Initialize()
        {
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

        public void RefreshData()
        {
            this.LoadingUnitsDataSource = null;
            this.LoadingUnitsDataSource = this.cell != null
                ? this.dataSourceService
                    .GetAll<LoadingUnitDetails>(nameof(CellDetailsViewModel), this.cell.Id)
                    .Single()
                : null;
        }
    }
}
