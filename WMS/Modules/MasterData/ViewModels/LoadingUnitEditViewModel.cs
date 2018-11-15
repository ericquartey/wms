using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
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
        private ICommand addBulkCommand;
        private ICommand addCommand;
        private bool addVisibility;
        private bool bulkAddVisibility;
        private ICommand cancelCommand;
        private int column;
        private IDataSource<CompartmentDetails> compartmentsDataSource;
        private bool createMode;
        private ICommand deleteCommand;
        private ICommand editCommand;
        private bool editMode;
        private bool editVisibility;
        private bool enableBulkAdd;
        private string error;
        private string errorColor;
        private bool isExpand;
        private bool isReadOnlyTray;
        private bool isSelectableTray;
        private bool isVisibleMainCommandBar;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private bool readOnlyTray;
        private int row;
        private ICommand saveCommand;
        private object selectedCompartment;
        private CompartmentDetails selectedCompartmentTray;
        private string title;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.IsSelectableTray = true;
            this.IsVisibleMainCommandBar = true;
            this.SelectedCompartmentTray = null;
            this.SelectedCompartment = null;
        }

        #endregion Constructors

        #region Properties

        public ICommand AddBulkCommand => this.addBulkCommand ??
                  (this.addBulkCommand = new DelegateCommand(this.ExecuteAddBulkCommand, this.CanExecuteAddBulkCommand)
            .ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode)
            .ObservesProperty(() => this.SelectedCompartmentTray.Width).ObservesProperty(() => this.SelectedCompartmentTray.Height)
            .ObservesProperty(() => this.Row).ObservesProperty(() => this.Column));

        public ICommand AddCommand => this.addCommand ??
                  (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand, this.CanExecuteAddCommand).ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        public bool AddVisibility
        {
            get { return this.addVisibility; }
            set { this.SetProperty(ref this.addVisibility, value); }
        }

        public bool BulkAddVisibility
        {
            get { return this.bulkAddVisibility; }
            set { this.SetProperty(ref this.bulkAddVisibility, value); }
        }

        public ICommand CancelCommand => this.cancelCommand ??
          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand, this.CanExecuteCancelCommand).ObservesProperty(() => this.CreateMode).ObservesProperty(() => this.EditMode));

        public int Column
        {
            get => this.column;
            set
            {
                this.SetProperty(ref this.column, value);
            }
        }

        public IDataSource<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public bool CreateMode
        {
            get => this.createMode;
            set
            {
                this.SetProperty(ref this.createMode, value);
            }
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
            set
            {
                this.SetProperty(ref this.enableBulkAdd, value);
            }
        }

        public string Error { get => this.error; set { this.SetProperty(ref this.error, value); } }

        public string ErrorColor { get => this.errorColor; set { this.SetProperty(ref this.errorColor, value); } }

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

        public int Row
        {
            get => this.row;
            set
            {
                this.SetProperty(ref this.row, value);
            }
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

        public string Title { get => this.title; set { this.SetProperty(ref this.title, value); } }

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

        private bool CanExecuteAddBulkCommand()
        {
            return !this.CreateMode && !this.EditMode;
        }

        private bool CanExecuteAddCommand()
        {
            return !this.CreateMode && !this.EditMode;
        }

        private bool CanExecuteCancelCommand()
        {
            return this.CreateMode || this.EditMode;
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

        private bool CanExecuteSaveCommand()
        {
            return this.EditMode || this.CreateMode;
        }

        private void EnableCreation()
        {
            this.SetError();
            this.SetSelectedCompartment(new CompartmentDetails());
            this.SelectedCompartment = null;
            this.CreateMode = true;
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsVisibleMainCommandBar = false;
        }

        private void EnableEdit()
        {
            this.SetError();
            this.EditMode = true;
            this.IsExpand = true;
            this.IsVisibleMainCommandBar = false;
        }

        private void ExecuteAddBulkCommand()
        {
            this.EnableBulkAdd = true;
            this.Row = 0;
            this.Column = 0;
            this.EnableCreation();
            this.SetFunctionPanel((int)commandCompartment.BulkAdd);
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.EnableCreation();
            this.SetFunctionPanel((int)commandCompartment.Add);
        }

        private void ExecuteCancelCommand()
        {
            this.SetError();
            this.CreateMode = false;
            this.EnableBulkAdd = false;
            this.EditMode = false;
            this.IsExpand = false;
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsVisibleMainCommandBar = true;
            this.SetFunctionPanel();
        }

        private void ExecuteDeleteCommand()
        {
            this.SetError();
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);
            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
            this.ExecuteCancelCommand();
        }

        private void ExecuteEditCompartmentCommand()
        {
            this.EnableEdit();
            this.SetFunctionPanel((int)commandCompartment.Edit);
        }

        private void ExecuteSaveCommand()
        {
            this.SetError();
            if (this.EnableBulkAdd)
            {
                if (this.GenerateBulkCompartments())
                {
                    this.ExecuteCancelCommand();
                }
            }
            else
            {
                if (this.SaveLoadingUnit())
                {
                    this.ExecuteCancelCommand();
                }
            }
        }

        private bool GenerateBulkCompartments()
        {
            bool ok = false;
            var tempTray = this.tray;
            this.SelectedCompartmentTray.LoadingUnitId = this.LoadingUnit.Id;
            this.SelectedCompartmentTray.CompartmentTypeId = 1;
            try
            {
                List<CompartmentDetails> newCompartments = tempTray.AddBulkCompartments(this.SelectedCompartmentTray, this.Row, this.Column);

                bool addAll = true;
                foreach (var compartment in newCompartments)
                {
                    int add = this.compartmentProvider.Add(compartment);
                    if (add != 1)
                    {
                        addAll = false;
                    }
                }
                if (addAll)
                {
                    this.Tray = tempTray;
                    ok = true;
                }
            }
            catch (Exception ex)
            {
                //TODO: validation error
                this.SetError(Errors.AddBulkNoPossible);
            }
            return ok;
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

        private bool SaveLoadingUnit()
        {
            bool ok = false;
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

        private void SetFunctionPanel(int? newCommand = null)
        {
            switch (newCommand)
            {
                case 1:
                    this.AddVisibility = true;
                    this.BulkAddVisibility = false;
                    this.EditVisibility = false;
                    this.Title = Common.Resources.MasterData.AddCompartment;
                    break;

                case 2:
                    this.AddVisibility = false;
                    this.BulkAddVisibility = true;
                    this.EditVisibility = false;
                    this.Title = Common.Resources.MasterData.AddBulkCompartment;
                    break;

                case 3:
                    this.AddVisibility = false;
                    this.BulkAddVisibility = false;
                    this.EditVisibility = true;
                    this.Title = Common.Resources.MasterData.EditCompartment;
                    break;

                default:
                    this.AddVisibility = false;
                    this.BulkAddVisibility = false;
                    this.EditVisibility = false;
                    this.Title = "";
                    break;
            }
        }

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
