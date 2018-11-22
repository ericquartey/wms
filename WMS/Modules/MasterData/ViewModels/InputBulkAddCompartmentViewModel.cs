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
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private ICommand cancelCommand;
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
        }

        #endregion Constructors

        #region Events

        public event EventHandler FinishEvent;

        #endregion Events

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

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
                                .ObservesProperty(() => this.Error));

        public BulkCompartment SelectedBulkCompartmentTray
        {
            get => this.selectedBulkCompartmentTray;
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
            if (this.EnableCheck)
            {
                string error = this.Tray.CanBulkAddCompartment(this.SelectedBulkCompartmentTray, this.Tray, true);
                this.SetError(error);
                return error != null && error.Trim() != "";
            }
            return true;
        }

        private bool CanExecuteSaveCommand()
        {
            if (!this.EnableCheck)
            {
                return true;
            }
            return (this.Error == null || this.Error.Trim() == "");
        }

        private void EnableCreation()
        {
            this.SetError();
        }

        private void ExecuteBulkAddCommand()
        {
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
            if (this.GenerateBulkCompartments())
            {
                this.ResetView();
            }
        }

        private bool GenerateBulkCompartments()
        {
            bool ok = false;
            try
            {
                var newCompartments = this.tray.BulkAddCompartments(this.SelectedBulkCompartmentTray);

                var addAll = true;
                foreach (var compartment in newCompartments)
                {
                    compartment.LoadingUnitId = this.tray.LoadingUnitId;
                    compartment.CompartmentTypeId = 2;
                    var add = this.compartmentProvider.Add(compartment);
                    if (add.Result != 1)
                    {
                        addAll = false;
                    }
                }
                if (addAll)
                {
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
            if (this.CanExecuteBulkAddCommand())
            {
                this.ExecuteBulkAddCommand();
            }
            this.CanExecuteSaveCommand();
        }

        private void Reset()
        {
            this.SetError();
            this.EnableCheck = false;
            this.EnableInputBulkAdd = false;
        }

        private void ResetView()
        {
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
