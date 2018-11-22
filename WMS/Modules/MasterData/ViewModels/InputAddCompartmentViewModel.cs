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
                          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

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
                                 .ObservesProperty(() => this.Error)
                                .ObservesProperty(() => this.SelectedCompartmentTray));

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
                return error != null && error.Trim() != "";
            }

            return true;
        }

        private bool CanExecuteSaveCommand()
        {
            if (!this.EnableCheck && this.IsNullSelectedCompartment())
            {
                return false;
            }
            return (this.Error == null || this.Error.Trim() == "");
        }

        private void EnableCreation()
        {
            this.SetError();
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

        private bool IsNullSelectedCompartment()
        {
            if (this.selectedCompartmentTray != null && this.selectedCompartmentTray.XPosition != null && this.selectedCompartmentTray.YPosition != null
                && this.selectedCompartmentTray.Width != null && this.selectedCompartmentTray.Height != null && this.selectedCompartmentTray.Width != 0 && this.selectedCompartmentTray.Height != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OnSelectedCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.CanExecuteAddCommand())
            {
                this.ExecuteAddCommand();
            }
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        private void Reset()
        {
            this.SetError();
            this.EnableCheck = false;
        }

        private void ResetView()
        {
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
                    this.tray.AddCompartment(this.SelectedCompartmentTray);
                    //this.SelectedCompartment = this.SelectedCompartmentTray;
                }
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
