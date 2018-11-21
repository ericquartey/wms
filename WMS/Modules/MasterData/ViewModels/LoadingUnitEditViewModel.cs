using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        //private bool bulkAddVisibility;
        private ICommand cancelCommandAdd;

        private ICommand cancelCommandEdit;

        //private int column;
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
        private InputBulkAddCompartmentViewModel inputBulkAddVM;
        private bool isEnabledGrid;

        private bool isExpand;

        private bool isReadOnlyTray;

        private bool isSelectableTray;

        private bool isVisibleMainCommandBar;

        private LoadingUnitDetails loadingUnit;

        private bool loadingUnitHasCompartments;

        private bool readOnlyTray;

        //private int row;
        private ICommand saveCommandAdd;

        private ICommand saveCommandEdit;

        private BulkCompartment selectedBulkCompartmentTray;

        private object selectedCompartment;

        private CompartmentDetails selectedCompartmentTray;

        private string titleAdd = Common.Resources.MasterData.AddCompartment;

        private string titleEdit = Common.Resources.MasterData.EditCompartment;

        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsVisibleMainCommandBar = true;
            this.SelectedCompartmentTray = null;
            this.SelectedCompartment = null;
            this.IsEnabledGrid = true;

            this.EnableInputBulkAdd = false;
        }

        #endregion Constructors

        #region Properties

        public ICommand AddCommand => this.addCommand ??
                  (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand, this.CanExecuteAddCommand).ObservesProperty(() => this.CreateMode));

        public bool AddVisibility
        {
            get { return this.addVisibility; }
            set { this.SetProperty(ref this.addVisibility, value); }
        }

        public ICommand BulkAddCommand => this.bulkAddCommand ??
                                  (this.bulkAddCommand = new DelegateCommand(this.ExecuteBulkAddCommand, this.CanExecuteBulkAddCommand)
            .ObservesProperty(() => this.CreateMode));

        public ICommand CancelCommandAdd => this.cancelCommandAdd ??
          (this.cancelCommandAdd = new DelegateCommand(this.ExecuteCancelCommandAdd, this.CanExecuteCancelCommandAdd).ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        //public bool BulkAddVisibility
        //{
        //    get { return this.bulkAddVisibility; }
        //    set { this.SetProperty(ref this.bulkAddVisibility, value); }
        //}
        public ICommand CancelCommandEdit => this.cancelCommandEdit ??
          (this.cancelCommandEdit = new DelegateCommand(this.ExecuteCancelCommandEdit, this.CanExecuteCancelCommandEdit).ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        public IDataSource<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        //public int Column
        //{
        //    get => this.column;
        //    set => this.SetProperty(ref this.column, value);
        //}
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

        public bool EditMode { get => this.editMode; set { this.SetProperty(ref this.editMode, value); } }

        public bool EditVisibility
        {
            get { return this.editVisibility; }
            set { this.SetProperty(ref this.editVisibility, value); }
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

        public InputAddCompartmentViewModel InputAddVM { get; set; }

        public InputBulkAddCompartmentViewModel InputBulkAddVM
        {
            get => this.inputBulkAddVM;
            set => this.SetProperty(ref this.inputBulkAddVM, value);
        }

        public InputEditCompartmentViewModel InputEditVM { get; set; }

        public bool IsEnabledGrid
        {
            get { return this.isEnabledGrid; }
            set { this.SetProperty(ref this.isEnabledGrid, value); }
        }

        public bool IsExpand
        {
            get { return this.isExpand; }
            set { this.SetProperty(ref this.isExpand, value); }
        }

        public bool IsReadOnlyTray
        {
            get { return this.isReadOnlyTray; }
            set { this.SetProperty(ref this.isReadOnlyTray, value); }
        }

        public bool IsSelectableTray
        {
            get { return this.isSelectableTray; }
            set { this.SetProperty(ref this.isSelectableTray, value); }
        }

        public bool IsVisibleMainCommandBar
        {
            get { return this.isVisibleMainCommandBar; }
            set { this.SetProperty(ref this.isVisibleMainCommandBar, value); }
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

        //public int Row
        //{
        //    get => this.row;
        //    set => this.SetProperty(ref this.row, value);
        //}

        public ICommand SaveCommandAdd => this.saveCommandAdd ??
                  (this.saveCommandAdd = new DelegateCommand(this.ExecuteSaveCommandAdd, this.CanExecuteSaveCommandAdd)
            .ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode)
            .ObservesProperty(() => this.Error));

        public ICommand SaveCommandEdit => this.saveCommandEdit ??
                  (this.saveCommandEdit = new DelegateCommand(this.ExecuteSaveCommandEdit, this.CanExecuteSaveCommandEdit)
            .ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode)
            .ObservesProperty(() => this.Error));

        public BulkCompartment SelectedBulkCompartmentTray
        {
            get
            {
                //if (this.EnableBulkAdd)
                //{
                //    this.CanExecuteBulkAddCommand();
                //}
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

        public string TitleAdd { get => this.titleAdd; private set => this.SetProperty(ref this.titleAdd, value); }
        public string TitleEdit { get => this.titleEdit; private set => this.SetProperty(ref this.titleEdit, value); }

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
            if (this.EnableCheck)
            {
                string error = this.Tray.CanBulkAddCompartment(this.SelectedCompartmentTray, this.Tray, true);
                this.SetError(error);
            }

            return !this.CreateMode && !this.EditMode;
        }

        private bool CanExecuteBulkAddCommand()
        {
            //if (this.EnableCheck && this.EnableBulkAdd)
            //{
            //    string error = this.Tray.CanBulkAddCompartment(this.SelectedBulkCompartmentTray, this.Tray, true);
            //    this.SetError(error);
            //}

            return !this.CreateMode && !this.EditMode;
        }

        private bool CanExecuteCancelCommandAdd()
        {
            return this.CreateMode;
        }

        private bool CanExecuteCancelCommandEdit()
        {
            return this.EditMode;
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null && !this.CreateMode;
        }

        private bool CanExecuteEditCommand()
        {
            return !this.CreateMode && !this.EditMode && this.selectedCompartmentTray != null
                && this.selectedCompartmentTray.Width != null && this.selectedCompartmentTray.Height != null
                && this.selectedCompartmentTray.XPosition != null && this.selectedCompartmentTray.YPosition != null;
        }

        private bool CanExecuteSaveCommandAdd()
        {
            bool x = this.CreateMode && (this.Error == null || this.Error.Trim().Equals(""));
            return this.CreateMode && (this.Error == null || this.Error.Trim() == "");// this.Error.Trim().Equals(""));
        }

        private bool CanExecuteSaveCommandEdit()
        {
            bool x = (this.EditMode) && (this.Error == null || this.Error.Trim().Equals(""));
            return this.EditMode && (this.Error == null || this.Error.Trim() == "");// this.Error.Trim().Equals(""));
        }

        private void EnableCreation()
        {
            this.SetError();
            this.SetSelectedCompartment(new CompartmentDetails());// { Width = 0, Height = 0, XPosition = 0, YPosition = 0 });
            this.SelectedCompartment = null;
            this.CreateMode = true;
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsEnabledGrid = false;
            this.IsVisibleMainCommandBar = false;
        }

        private void EnableEdit()
        {
            this.SetError();
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
            //this.SetFunctionPanel((int)commandCompartment.Add);
            //this.SelectedCompartmentTray.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;

            //NEW
            //this.InputAddVM.Initialize(this.tray, this.loadingUnit.Id);
            //this.InputAddVM.FinishEvent += this.InputAddVM_FinishEvent;
        }

        private void ExecuteBulkAddCommand()
        {
            this.EnableBulkAdd = true;
            //this.BulkAddVisibility = true;
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsEnabledGrid = false;
            this.IsVisibleMainCommandBar = false;
            //this.EnableCreation();
            //this.SetFunctionPanel((int)commandCompartment.BulkAdd);

            this.EnableInputBulkAdd = true;

            //NEW
            this.InputBulkAddVM.Initialize(this.tray, this.loadingUnit.Id);
            this.InputBulkAddVM.FinishEvent += this.InputBulkAddVM_FinishEvent;
        }

        private void ExecuteCancelCommandAdd()
        {
            this.CreateMode = false;
            this.SelectedCompartmentTray.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.ResetInputView();
        }

        private void ExecuteCancelCommandEdit()
        {
            this.EditMode = false;
            this.SelectedCompartmentTray.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.ResetInputView();
        }

        private void ExecuteDeleteCommand()
        {
            this.SetError();
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);
            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
            this.ResetInputView();
        }

        private void ExecuteEditCompartmentCommand()
        {
            this.EnableEdit();
            //this.SetFunctionPanel((int)commandCompartment.Edit);
            this.SelectedCompartmentTray.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;
        }

        private void ExecuteSaveCommandAdd()
        {
            this.SetError();
            this.EnableCheck = true;

            if (this.SaveLoadingUnit())
            {
                this.ResetInputView();
            }
        }

        private void ExecuteSaveCommandEdit()
        {
            this.SetError();
            this.EnableCheck = true;

            if (this.SaveLoadingUnit())
            {
                this.ResetInputView();
            }
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

        private void InputBulkAddVM_FinishEvent(Object sender, EventArgs e)
        {
            //
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
            this.CanExecuteSaveCommandAdd();
        }

        private void ResetInputView()
        {
            this.SetError();
            this.IsExpand = false;
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsEnabledGrid = true;
            this.IsVisibleMainCommandBar = true;
            this.EnableCheck = false;
            //this.SetFunctionPanel();
        }

        private bool SaveLoadingUnit()
        {
            bool ok = false;
            if (this.CreateMode)
            {
                if (this.tray.CanAddCompartment(this.SelectedCompartmentTray))
                {
                    this.SelectedCompartmentTray.LoadingUnitId = this.LoadingUnit.Id;
                    this.SelectedCompartmentTray.CompartmentTypeId = 2;

                    var add = this.compartmentProvider.Add(this.SelectedCompartmentTray);
                    if (add == 1)
                    {
                        this.tray.Compartments.Add(this.SelectedCompartmentTray);
                        this.SelectedCompartment = this.SelectedCompartmentTray;
                    }
                    this.CreateMode = false;

                    ok = true;
                }
                else
                {
                    this.SetError(Errors.AddNoPossible);
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
            return ok;
        }

        private void SetError(string message = null)
        {
            if (message == null)
            {
                this.Error = "";
                this.ErrorColor = Colors.Black.ToString();
            }
            else
            {
                this.Error = message;
                this.ErrorColor = Colors.Red.ToString();
            }
        }

        //private void SetFunctionPanel(int? newCommand = null)
        //{
        //    //switch (newCommand)
        //    //{
        //    //    case 1:
        //    //        this.AddVisibility = true;
        //    //        this.BulkAddVisibility = false;
        //    //        this.EditVisibility = false;
        //    //        //this.Title = Common.Resources.MasterData.AddCompartment;
        //    //        break;

        //    //    case 2:
        //    //        this.AddVisibility = false;
        //    //        this.BulkAddVisibility = true;
        //    //        this.EditVisibility = false;
        //    //        //this.Title = Common.Resources.MasterData.BulkAddCompartment;
        //    //        break;

        //    //    case 3:
        //    //        this.AddVisibility = false;
        //    //        this.BulkAddVisibility = false;
        //    //        this.EditVisibility = true;
        //    //        //this.Title = Common.Resources.MasterData.EditCompartment;
        //    //        break;

        //    //    default:
        //    //        this.AddVisibility = false;
        //    //        this.BulkAddVisibility = false;
        //    //        this.EditVisibility = false;
        //    //        //this.Title = "";
        //    //        break;
        //    //}
        //}

        private void SetSelectedCompartment(object value)
        {
            if (value is CompartmentDetails compartmentDetails)
            {
                this.SelectedCompartmentTray = compartmentDetails;
            }
        }

        #endregion Methods
    }
}
