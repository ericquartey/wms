using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class InputBulkAddCompartmentViewModel : BaseServiceNavigationViewModel
    {
        //private ICommand bulkAddCommand;

        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        //private bool bulkAddVisibility;
        private ICommand cancelCommand;

        private bool createMode;
        private bool enableBulkAdd;
        private bool enableInputBulkAdd;
        private string error;
        private string errorColor;
        private ICommand saveCommand;
        private BulkCompartment selectedBulkCompartmentTray;
        private string title = Common.Resources.MasterData.BulkAddCompartment;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public InputBulkAddCompartmentViewModel()
        {
            //this.InitializeBulkCompartment();
        }

        #endregion Constructors

        //public bool BulkAddVisibility
        //{
        //    get { return this.bulkAddVisibility; }
        //    set { this.SetProperty(ref this.bulkAddVisibility, value); }
        //}

        #region Events

        public event EventHandler FinishEvent;

        #endregion Events

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand, this.CanExecuteCancelCommand).ObservesProperty(() => this.CreateMode));

        public bool CreateMode
        {
            get => this.createMode;
            set => this.SetProperty(ref this.createMode, value);
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

        public ICommand SaveCommand => this.saveCommand ??
                                 (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSaveCommand)
           .ObservesProperty(() => this.CreateMode)
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

        public string Title { get => this.title; private set => this.SetProperty(ref this.title, value); }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public void Initialize(Tray tray)
        {
            this.Tray = tray;
            this.SelectedBulkCompartmentTray = new BulkCompartment();
            this.EnableInputBulkAdd = true;
            this.SelectedBulkCompartmentTray.PropertyChanged += this.OnSelectedBulkCompartmentPropertyChanged;
        }

        protected virtual void OnFinishEvent(EventArgs e)
        {
            EventHandler handler = FinishEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool CanExecuteBulkAddCommand()
        {
            if (this.EnableCheck)// && this.EnableBulkAdd)
            {
                string error = this.Tray.CanBulkAddCompartment(this.SelectedBulkCompartmentTray, this.Tray, true);
                this.SetError(error);
            }

            return !this.CreateMode;
        }

        private bool CanExecuteCancelCommand()
        {
            return true;// this.CreateMode;
        }

        private bool CanExecuteSaveCommand()
        {
            //if (this.EnableBulkAdd)
            //{
            if (!this.EnableCheck)
            {
                return true;
            }
            //if (this.SelectedBulkCompartmentTray.Row == 0 && this.SelectedBulkCompartmentTray.Column == 0)
            //{
            //    return false;
            //}
            //}
            bool x = this.CreateMode && (this.Error == null || this.Error.Trim().Equals(""));
            return this.CreateMode && (this.Error == null || this.Error.Trim() == "");// this.Error.Trim().Equals(""));
        }

        private void EnableCreation()
        {
            this.SetError();
            //this.InitializeBulkCompartment();
            //this.SetSelectedCompartment(new CompartmentDetails());// { Width = 0, Height = 0, XPosition = 0, YPosition = 0 });
            //this.SelectedCompartment = null;
            this.CreateMode = true;
            //this.IsExpand = true;
            //this.IsSelectableTray = false;
            //this.ReadOnlyTray = true;
            //this.IsEnabledGrid = false;
            //this.IsVisibleMainCommandBar = false;
        }

        private void ExecuteBulkAddCommand()
        {
            this.EnableBulkAdd = true;
            this.EnableCreation();
            this.EnableInputBulkAdd = true;
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
        }

        private void ExecuteFinishCommand()
        {
            //TO PARENT UPDATE
        }

        private void ExecuteSaveCommand()
        {
            this.SetError();
            this.EnableCheck = true;
            if (this.EnableBulkAdd)
            {
                if (this.GenerateBulkCompartments())
                {
                    this.ResetView();
                }
            }
        }

        private bool GenerateBulkCompartments()
        {
            bool ok = false;
            //var tempTray = this.tray;
            //this.SelectedBulkCompartmentTray.LoadingUnitId = this.LoadingUnit.Id;
            //this.SelectedBulkCompartmentTray.CompartmentTypeId = 1;
            try
            {
                var newCompartments = this.tray.BulkAddCompartments(this.SelectedBulkCompartmentTray);

                var addAll = true;
                foreach (var compartment in newCompartments)
                {
                    compartment.LoadingUnitId = this.tray.LoadingUnitId;
                    compartment.CompartmentTypeId = 2;
                    var add = this.compartmentProvider.Add(compartment);
                    if (add != 1)
                    {
                        addAll = false;
                    }
                }
                if (addAll)
                {
                    //this.Tray = tempTray;
                    ok = true;
                }
            }
            catch (Exception ex)
            {
                //TODO: validation error
                this.SetError(Errors.BulkAddNoPossible + " " + ex.InnerException);
            }
            return ok;
        }

        private void OnSelectedBulkCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //((DelegateCommand)this.BulkAddCommand)?.RaiseCanExecuteChanged();
            if (this.CanExecuteBulkAddCommand())
            {
                this.ExecuteBulkAddCommand();
            }
            this.CanExecuteSaveCommand();
        }

        //private void InitializeBulkCompartment()
        //{
        //    this.SelectedBulkCompartmentTray = new BulkCompartment();
        //    //this.SelectedBulkCompartmentTray.CompartmentDetails = new CompartmentDetails();
        //    //this.SelectedBulkCompartmentTray.Row = 0;
        //    //this.SelectedBulkCompartmentTray.Column = 0;
        //    //this.SelectedBulkCompartmentTray.Width = null;
        //    //this.SelectedBulkCompartmentTray.Height = null;
        //    //this.SelectedBulkCompartmentTray.Row = 0;
        //    //this.SelectedBulkCompartmentTray.XPosition = 0;
        //    //this.SelectedBulkCompartmentTray.YPosition = 0;
        //    this.SelectedBulkCompartmentTray.PropertyChanged += this.OnSelectedBulkCompartmentPropertyChanged;
        //}
        private void Reset()
        {
            this.SetError();
            //this.IsExpand = false;
            //this.IsSelectableTray = true;
            //this.ReadOnlyTray = false;
            //this.IsEnabledGrid = true;
            //this.IsVisibleMainCommandBar = true;
            this.EnableCheck = false;
            this.EnableInputBulkAdd = false;
        }

        private void ResetView()
        {
            this.CreateMode = false;
            this.EnableBulkAdd = false;
            this.EnableInputBulkAdd = false;
            this.SelectedBulkCompartmentTray.PropertyChanged -= this.OnSelectedBulkCompartmentPropertyChanged;
            this.Reset();
            this.OnFinishEvent(null);
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

        #endregion Methods
    }
}
