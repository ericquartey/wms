using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private IDataSource<CompartmentDetails, int> compartmentsDataSource;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private object modelSelectionChangedSubscription;
        private ICommand saveCommand;
        private object selectedCompartment;
        private ICommand viewCompartmentDetailsCommand;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<CompartmentDetails, int> CompartmentsDataSource
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
                return (CompartmentDetails)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedCompartment).OriginalRow);
            }
        }

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set
            {
                if (this.SetProperty(ref this.loadingUnit, value))
                {
                    if (this.loadingUnit != null)
                    {
                        this.CompartmentsDataSource = this.dataSourceService.GetAll<CompartmentDetails, int>(nameof(LoadingUnitDetailsViewModel), this.loadingUnit.Id).Single();
                    }
                    else
                    {
                        this.CompartmentsDataSource = null;
                    }
                }
            }
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public object SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public ICommand ViewCompartmentDetailsCommand => this.viewCompartmentDetailsCommand ??
                                                         (this.viewCompartmentDetailsCommand = new DelegateCommand(this.ExecuteViewCompartmentDetailsCommand));

        #endregion Properties

        #region Methods

        public void ExecuteViewCompartmentDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTDETAILS, this.CurrentCompartment?.Id);
        }

        protected override void OnAppear()
        {
            if (this.Data is int modelId)
            {
                this.LoadData(modelId);
            }

            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<CompartmentDetails, int>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.loadingUnitProvider.Save(this.LoadingUnit);

            if (modifiedRowCount > 0)
            {
                this.EventService.Invoke(new ModelChangedEvent<LoadingUnitDetails, int>(this.LoadingUnit.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<LoadingUnitDetails, int>>(
                eventArgs =>
                {
                    if (eventArgs.ModelIdHasValue)
                    {
                        this.LoadData(eventArgs.ModelId);
                    }
                    else
                    {
                        this.LoadingUnit = null;
                    }
                },
                true);
        }

        private void LoadData(int modelId)
        {
            this.LoadingUnit = this.loadingUnitProvider.GetById(modelId);

            this.LoadingUnitHasCompartments = this.loadingUnitProvider.HasAnyCompartments(modelId);
        }

        #endregion Methods
    }
}
