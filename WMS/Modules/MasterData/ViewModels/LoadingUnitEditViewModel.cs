using System;
using System.Diagnostics;
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
    public class LoadingUnitEditViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private ICommand addCompartmentCommand;
        private IDataSource<CompartmentDetails> compartmentsDataSource;
        private ICommand deleteCommand;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private bool readOnlyTray;
        private ICommand saveCommand;
        private object selectedCompartment;
        private CompartmentDetails selectedCompartmentTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ICommand AddCompartmentCommand => this.addCompartmentCommand ??
          (this.addCompartmentCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand, this.CanExecuteAddCommand).ObservesProperty(() => this.CreateMode));

        public IDataSource<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public bool CreateMode { get; set; }

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

        public ICommand DeleteCommand => this.deleteCommand ??
          (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedCompartmentTray).ObservesProperty(() => this.CreateMode));

        public bool EditMode { get; set; }

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

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSaveCommand).ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        public object SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.SetProperty(ref this.selectedCompartment, value);
                this.RaisePropertyChanged(nameof(this.CurrentCompartment));

                this.SetSelectedCompartment(this.CurrentCompartment);
            }
        }

        public CompartmentDetails SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = null;
            this.CompartmentsDataSource = this.loadingUnit != null
                ? this.dataSourceService
                    .GetAll<CompartmentDetails>(nameof(LoadingUnitDetailsViewModel), this.loadingUnit.Id)
                    .Single()
                : null;
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        private bool CanExecuteAddCommand()
        {
            return !this.CreateMode;
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null && !this.CreateMode;
        }

        private bool CanExecuteSaveCommand()
        {
            return this.EditMode || this.CreateMode;
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.selectedCompartmentTray = new CompartmentDetails();
            this.RaisePropertyChanged(nameof(this.SelectedCompartmentTray));
            this.CreateMode = true;
            this.RaisePropertyChanged(nameof(this.CreateMode));
        }

        private void ExecuteDeleteCommand()
        {
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);

            //ToDo: implement save/update/delete
            //this.SaveLoadingUnit();

            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
        }

        private void ExecuteSaveCommand()
        {
            this.SaveLoadingUnit();
        }

        private void Initialize()
        {
        }

        private void InitializeTray()
        {
            this.tray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = this.LoadingUnit.Length,
                    Width = this.LoadingUnit.Width
                }
            };
            if (this.LoadingUnit.Compartments != null)
            {
                this.tray.AddCompartmentsRange(this.LoadingUnit.Compartments);
            }
            this.RaisePropertyChanged(nameof(this.Tray));
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.LoadingUnit = this.loadingUnitProvider.GetById(modelId);

                this.InitializeTray();
            }
        }

        private void SaveLoadingUnit()
        {
            if (this.CreateMode)
            {
                if (this.tray.CanAddCompartment(this.SelectedCompartmentTray))
                {
                    this.SelectedCompartmentTray.LoadingUnitId = this.LoadingUnit.Id;
                    this.SelectedCompartmentTray.CompartmentTypeId = 2;

                    int add = this.compartmentProvider.Add(this.SelectedCompartmentTray);
                    if (add == 1)
                    {
                        this.tray.Compartments.Add(this.SelectedCompartmentTray);
                    }
                    this.CreateMode = false;
                    this.RaisePropertyChanged(nameof(this.CreateMode));

                    Debug.WriteLine($"Add NEW Compartment: {add}");
                }
            }
            else
            {
                var modifiedRowCount = this.loadingUnitProvider.Save(this.LoadingUnit);

                if (modifiedRowCount > 0)
                {
                    this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.LoadingUnit.Id));

                    this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
                }
            }
        }

        private void SetSelectedCompartment(object value)
        {
            if (value is CompartmentDetails compartmentDetails)
            {
                this.selectedCompartmentTray = compartmentDetails;
                this.RaisePropertyChanged(nameof(this.SelectedCompartmentTray));
            }
        }

        #endregion Methods
    }
}
