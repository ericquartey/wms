using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public enum commandCompartment
    {
        Add = 1,
        BulkAdd = 2,
        Edit = 3
    }

    public class LoadingUnitEditViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private ICommand addCommand;
        private bool addVisibility;
        private ICommand bulkAddCommand;
        private IDataSource<CompartmentDetails> compartmentsDataSource;
        private bool createMode;
        private ICommand deleteCommand;
        private ICommand editCommand;
        private bool editMode;
        private bool editVisibility;
        private bool enableBulkAdd;
        private bool enableInputBulkAdd;
        private string error;
        private string errorColor;
        private Func<CompartmentDetails, CompartmentDetails, string> filterColorFunc;
        private InputAddCompartmentViewModel inputAddVM;
        private InputBulkAddCompartmentViewModel inputBulkAddVM;
        private InputEditCompartmentViewModel inputEditVM;
        private bool isEnabledGrid;

        private bool isExpand;

        private bool isReadOnlyTray;

        private bool isSelectableTray;

        private bool isVisibleMainCommandBar;

        private LoadingUnitDetails loadingUnit;

        private bool loadingUnitHasCompartments;

        private bool readOnlyTray;

        private BulkCompartment selectedBulkCompartmentTray;

        private object selectedCompartment;

        private CompartmentDetails selectedCompartmentTray;

        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.ShowMainViewAndHideLateralPanel();
        }

        #endregion Constructors

        #region Properties

        public ICommand AddCommand => this.addCommand ??
                  (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand).ObservesProperty(() => this.CreateMode));

        public bool AddVisibility
        {
            get => this.addVisibility;
            set => this.SetProperty(ref this.addVisibility, value);
        }

        public ICommand BulkAddCommand => this.bulkAddCommand ??
                                  (this.bulkAddCommand = new DelegateCommand(this.ExecuteBulkAddCommand)
            .ObservesProperty(() => this.CreateMode));

        public IDataSource<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public bool CreateMode
        {
            get => this.createMode;
            set => this.SetProperty(ref this.createMode, value);
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

        public ICommand DeleteCommand => this.deleteCommand ??
          (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedCompartmentTray)
            .ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        public ICommand EditCommand => this.editCommand ??
          (this.editCommand = new DelegateCommand(this.ExecuteEditCompartmentCommand, this.CanExecuteEditCommand)
            .ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode)
            .ObservesProperty(() => this.SelectedCompartmentTray));

        public bool EditMode { get => this.editMode; set => this.SetProperty(ref this.editMode, value); }

        public bool EditVisibility
        {
            get => this.editVisibility;
            set => this.SetProperty(ref this.editVisibility, value);
        }

        public bool EnableBulkAdd
        {
            get => this.enableBulkAdd;
            set => this.SetProperty(ref this.enableBulkAdd, value);
        }

        public bool EnableCheck { get; set; }

        public bool EnableInputBulkAdd
        {
            get { return this.enableInputBulkAdd; }
            set { this.SetProperty(ref this.enableInputBulkAdd, value); }
        }

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public string ErrorColor { get => this.errorColor; set => this.SetProperty(ref this.errorColor, value); }

        public Func<CompartmentDetails, CompartmentDetails, string> FilterColorFunc
        {
            get { return this.filterColorFunc; }
            set { this.SetProperty(ref this.filterColorFunc, value); }
        }

        public InputAddCompartmentViewModel InputAddVM
        {
            get => this.inputAddVM;
            set => this.SetProperty(ref this.inputAddVM, value);
        }

        public InputBulkAddCompartmentViewModel InputBulkAddVM
        {
            get => this.inputBulkAddVM;
            set => this.SetProperty(ref this.inputBulkAddVM, value);
        }

        public InputEditCompartmentViewModel InputEditVM
        {
            get => this.inputEditVM;
            set => this.SetProperty(ref this.inputEditVM, value);
        }

        public bool IsEnabledGrid
        {
            get => this.isEnabledGrid;
            set => this.SetProperty(ref this.isEnabledGrid, value);
        }

        public bool IsExpand
        {
            get => this.isExpand;
            set => this.SetProperty(ref this.isExpand, value);
        }

        public bool IsReadOnlyTray
        {
            get => this.isReadOnlyTray;
            set => this.SetProperty(ref this.isReadOnlyTray, value);
        }

        public bool IsSelectableTray
        {
            get => this.isSelectableTray;
            set => this.SetProperty(ref this.isSelectableTray, value);
        }

        public bool IsVisibleMainCommandBar
        {
            get => this.isVisibleMainCommandBar;
            set => this.SetProperty(ref this.isVisibleMainCommandBar, value);
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

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        public BulkCompartment SelectedBulkCompartmentTray
        {
            get
            {
                return this.selectedBulkCompartmentTray;
            }
            set => this.SetProperty(ref this.selectedBulkCompartmentTray, value);
        }

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

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null && !this.CreateMode;
        }

        private bool CanExecuteEditCommand()
        {
            return (this.selectedCompartmentTray != null && this.selectedCompartmentTray.Width != null && this.selectedCompartmentTray.Height != null
                && this.selectedCompartmentTray.XPosition != null && this.selectedCompartmentTray.YPosition != null);
        }

        private void EnableCreation()
        {
            this.SetSelectedCompartment(new CompartmentDetails());
            this.SelectedCompartment = null;
            this.CreateMode = true;
        }

        private void EnableEdit()
        {
            this.EditMode = true;
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsEnabledGrid = false;
            this.IsVisibleMainCommandBar = false;
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.EnableCreation();
            this.HideMainViewAndShowLateralPanel();

            //NEW
            this.InputAddVM.Initialize(this.tray, this.loadingUnit.Id);
            this.InputAddVM.FinishEvent += this.InputAddVM_FinishEvent;
        }

        private void ExecuteBulkAddCommand()
        {
            this.EnableBulkAdd = true;
            this.EnableCreation();
            this.HideMainViewAndShowLateralPanel();

            this.EnableInputBulkAdd = true;

            //NEW
            this.InputBulkAddVM.Initialize(this.tray);
            this.InputBulkAddVM.FinishEvent += this.InputBulkAddVM_FinishEvent;
        }

        private void ExecuteDeleteCommand()
        {
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);
            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
            this.ResetInputView();
        }

        private void ExecuteEditCompartmentCommand()
        {
            this.EnableEdit();
            this.EnableEdit();
            this.HideMainViewAndShowLateralPanel();
            //NEW
            this.InputEditVM.Initialize(this.tray, this.loadingUnit, this.selectedCompartmentTray);
            this.InputEditVM.FinishEvent += this.InputEditVM_FinishEvent;
        }

        private void HideMainViewAndShowLateralPanel()
        {
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsEnabledGrid = false;
            this.IsVisibleMainCommandBar = false;
        }

        private void InitializeTray()
        {
            this.tray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = this.LoadingUnit.Length,
                    Width = this.LoadingUnit.Width
                },
                LoadingUnitId = this.LoadingUnit.Id,
            };
            if (this.LoadingUnit.Compartments != null)
            {
                this.tray.AddCompartmentsRange(this.LoadingUnit.Compartments);
            }

            this.FilterColorFunc = (new EditFilter()).ColorFunc;

            this.RaisePropertyChanged(nameof(this.Tray));
        }

        private void InputAddVM_FinishEvent(Object sender, EventArgs e)
        {
            this.ResetInputView();
        }

        private void InputBulkAddVM_FinishEvent(Object sender, EventArgs e)
        {
            //
            this.ResetInputView();
        }

        private void InputEditVM_FinishEvent(Object sender, EventArgs e)
        {
            this.ResetInputView();
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.LoadingUnit = this.loadingUnitProvider.GetById(modelId);

                this.InitializeTray();
            }
        }

        private void OnSelectedCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.AddCommand)?.RaiseCanExecuteChanged();
        }

        private void ResetInputView()
        {
            this.IsExpand = false;
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsEnabledGrid = true;
            this.IsVisibleMainCommandBar = true;
            this.EnableCheck = false;
        }

        private void SetSelectedCompartment(object value)
        {
            if (value is CompartmentDetails compartmentDetails)
            {
                this.SelectedCompartmentTray = compartmentDetails;
            }
        }

        private void ShowMainViewAndHideLateralPanel()
        {
            this.IsExpand = false;
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsEnabledGrid = true;
            this.IsVisibleMainCommandBar = true;
        }

        #endregion Methods
    }
}
