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
        private ICommand addBulkCommand;
        private ICommand addCommand;
        private ICommand cancelCommand;
        private int column;
        private IDataSource<CompartmentDetails> compartmentsDataSource;
        private bool createMode;
        private ICommand deleteCommand;
        private bool enableBulkAdd;
        private string error;
        private string errorColor;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private bool readOnlyTray;
        private int row;
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

        public ICommand AddBulkCommand => this.addBulkCommand ??
          (this.addBulkCommand = new DelegateCommand(this.ExecuteAddBulkCommand, this.CanExecuteAddBulkCommand).ObservesProperty(() => this.CreateMode));

        public ICommand AddCommand => this.addCommand ??
                  (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand, this.CanExecuteAddCommand).ObservesProperty(() => this.CreateMode));

        public ICommand CancelCommand => this.cancelCommand ??
          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand, this.CanExecuteCancelCommand).ObservesProperty(() => this.CreateMode));

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
          (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedCompartmentTray).ObservesProperty(() => this.CreateMode));

        public bool EditMode { get; set; }

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

        private Boolean CanExecuteAddBulkCommand()
        {
            return !this.CreateMode;
        }

        private bool CanExecuteAddCommand()
        {
            return !this.CreateMode;
        }

        private bool CanExecuteCancelCommand()
        {
            return this.CreateMode;
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null && !this.CreateMode;
        }

        private bool CanExecuteSaveCommand()
        {
            return this.EditMode || this.CreateMode;
        }

        private void ExecuteAddBulkCommand()
        {
            this.SetError(null);
            this.EnableBulkAdd = true;
            this.CreateMode = true;
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.SetError(null);
            this.SetSelectedCompartment(new CompartmentDetails());
            this.CreateMode = true;
        }

        private void ExecuteCancelCommand()
        {
            this.SetError(null);
            this.CreateMode = false;
            if (this.EnableBulkAdd)
            {
                this.EnableBulkAdd = false;
            }
        }

        private void ExecuteDeleteCommand()
        {
            this.SetError(null);
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);
            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
        }

        private void ExecuteSaveCommand()
        {
            this.SetError(null);
            if (this.EnableBulkAdd)
            {
                this.GenerateBulkCompartments();
            }
            else
            {
                this.SaveLoadingUnit();
            }
        }

        private void GenerateBulkCompartments()
        {
            var tempTray = this.tray;
            this.SelectedCompartmentTray.LoadingUnitId = this.LoadingUnit.Id;
            this.SelectedCompartmentTray.CompartmentTypeId = 1;
            List<CompartmentDetails> newCompartments = tempTray.AddBulkCompartments(this.SelectedCompartmentTray, this.Row, this.Column);

            if (newCompartments != null)
            {
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
                }
            }
            else
            {
                //ToDO: Dialog error
                this.SetError("Error: it is no possible to Add Bulk Compartments.");
            }
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

        private void SetError(string message)
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
