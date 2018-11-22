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
    public class InputAddCompartmentViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private ICommand cancelCommand;
        private bool createMode;
        private bool enableInputAdd;
        private string error;
        private string errorColor;
        private int loadingUnitId;
        private ICommand saveCommand;
        private CompartmentDetails selectedCompartmentTray;
        private string title = Common.Resources.MasterData.AddCompartment;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public InputAddCompartmentViewModel()
        {
        }

        #endregion Constructors

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

        public bool EnableCheck { get; set; }

        public bool EnableInputAdd
        {
            get { return this.enableInputAdd; }
            set { this.SetProperty(ref this.enableInputAdd, value); }
        }

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public string ErrorColor { get => this.errorColor; set => this.SetProperty(ref this.errorColor, value); }

        public ICommand SaveCommand => this.saveCommand ??
                                 (this.saveCommand = new DelegateCommand(() => this.ExecuteSaveCommand().ConfigureAwait(false), this.CanExecuteSaveCommand)
           .ObservesProperty(() => this.CreateMode)
           .ObservesProperty(() => this.Error));

        public CompartmentDetails SelectedCompartmentTray
        {
            get
            {
                return this.selectedCompartmentTray;
            }
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        public string Title { get => this.title; private set => this.SetProperty(ref this.title, value); }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public void Initialize(Tray tray, int loadingUnitId)
        {
            this.Tray = tray;
            this.SelectedCompartmentTray = new CompartmentDetails();
            this.EnableInputAdd = true;
            this.SelectedCompartmentTray.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;
            this.loadingUnitId = loadingUnitId;
        }

        protected virtual void OnFinishEvent(EventArgs e)
        {
            EventHandler handler = FinishEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool CanExecuteAddCommand()
        {
            if (this.EnableCheck)
            {
                string error = this.Tray.CanBulkAddCompartment(this.SelectedCompartmentTray, this.Tray, true);
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
            if (!this.EnableCheck)
            {
                return true;
            }
            bool x = this.CreateMode && (this.Error == null || this.Error.Trim().Equals(""));
            return this.CreateMode && (this.Error == null || this.Error.Trim() == "");// this.Error.Trim().Equals(""));
        }

        private void EnableCreation()
        {
            this.SetError();
            //this.SetSelectedCompartment(new CompartmentDetails());// { Width = 0, Height = 0, XPosition = 0, YPosition = 0 });
            //this.SelectedCompartment = null;
            this.CreateMode = true;
        }

        private void ExecuteAddCommand()
        {
            this.EnableCreation();
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
        }

        private void ExecuteFinishCommand()
        {
            //TO PARENT UPDATE
        }

        private async Task ExecuteSaveCommand()
        {
            this.SetError();
            this.EnableCheck = true;
            if (await this.SaveLoadingUnit())
            {
                this.ResetView();
            }
        }

        private void OnSelectedCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.CanExecuteAddCommand())
            {
                this.ExecuteAddCommand();
            }
            this.CanExecuteSaveCommand();
        }

        private void Reset()
        {
            this.SetError();
            this.EnableCheck = false;
        }

        private void ResetView()
        {
            this.CreateMode = false;
            this.EnableInputAdd = false;
            this.SelectedCompartmentTray.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.Reset();
            this.OnFinishEvent(null);
        }

        private async Task<bool> SaveLoadingUnit()
        {
            bool ok = false;
            if (this.tray.CanAddCompartment(this.SelectedCompartmentTray))
            {
                this.SelectedCompartmentTray.LoadingUnitId = this.loadingUnitId;
                this.SelectedCompartmentTray.CompartmentTypeId = 2;

                var add = await this.compartmentProvider.Add(this.SelectedCompartmentTray);
                if (add == 1)
                {
                    this.tray.Compartments.Add(this.SelectedCompartmentTray);
                    //this.SelectedCompartment = this.SelectedCompartmentTray;
                }
                this.CreateMode = false;

                ok = true;
            }
            else
            {
                this.SetError(Errors.AddNoPossible);
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

        #endregion Methods
    }
}
