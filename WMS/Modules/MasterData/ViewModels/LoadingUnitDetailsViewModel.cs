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
    public class LoadingUnitDetailsViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private IDataSource<CompartmentDetails> compartmentsDataSource;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private object modelSelectionChangedSubscription;
        private ICommand revertCommand;
        private ICommand saveCommand;
        private object selectedCompartment;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public CompartmentDetails CurrentCompartment
        {
            get
            {
                if (this.selectedCompartment == null)
                {
                    return default(CompartmentDetails);
                }
                if ((this.selectedCompartment is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(CompartmentDetails);
                }
                return (CompartmentDetails)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedCompartment).OriginalRow);
            }
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
                this.RefreshData();
            }
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
          (this.revertCommand = new DelegateCommand(this.LoadData));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public object SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.SetProperty(ref this.selectedCompartment, value);
                this.RaisePropertyChanged(nameof(this.CurrentCompartment));
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
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<LoadingUnit>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.loadingUnitProvider.Save(this.LoadingUnit);

            if (modifiedRowCount > 0)
            {
                this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.LoadingUnit.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<LoadingUnit>>(
                eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        this.LoadData();
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

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.LoadingUnit = this.loadingUnitProvider.GetById(modelId);
                this.LoadingUnitHasCompartments = this.loadingUnitProvider.HasAnyCompartments(modelId);
            }
        }

        #endregion Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = null;
            this.CompartmentsDataSource = this.loadingUnit != null
                ? this.dataSourceService
                    .GetAll<CompartmentDetails>(nameof(LoadingUnitDetailsViewModel), this.loadingUnit.Id)
                    .Single()
                : null;
        }
    }
}
