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
            get => this.enableInputBulkAdd;
            set => this.SetProperty(ref this.enableInputBulkAdd, value);
        }

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public string ErrorColor { get => this.errorColor; set => this.SetProperty(ref this.errorColor, value); }

        public ICommand SaveCommand => this.saveCommand ??
                                 (this.saveCommand = new DelegateCommand(() => this.ExecuteSaveCommand().ConfigureAwait(false), this.CanExecuteSaveCommand)
                                .ObservesProperty(() => this.Error)
                                .ObservesProperty(() => this.SelectedBulkCompartmentTray));

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
            EventHandler handler = this.FinishEvent;
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
                return error == null || error.Trim() == "";
            }
            return true;
        }

        private bool CanExecuteSaveCommand()
        {
            if (!this.EnableCheck && this.IsNullSelectedBulkCompartment())
            {
                return false;
            }
            return (this.Error == null || this.Error.Trim() == "");
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
        }

        private async Task ExecuteSaveCommand()
        {
            this.SetError();
            this.EnableCheck = true;
            if (await this.GenerateBulkCompartments())
            {
                this.ResetView();
            }
        }

        private async Task<bool> GenerateBulkCompartments()
        {
            try
            {
                var newCompartments = this.tray.BulkAddCompartments(this.SelectedBulkCompartmentTray);
                if (newCompartments == null || newCompartments.Count <= 0)
                {
                    return false;
                }
                var addAll = true;
                foreach (var compartment in newCompartments)
                {
                    compartment.LoadingUnitId = this.tray.LoadingUnitId;
                    compartment.CompartmentTypeId = 2;
                    var add = await this.compartmentProvider.Add(compartment);
                    if (add != 1)
                    {
                        addAll = false;
                    }
                }
                if (addAll)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                //TODO: validation error
                this.SetError(Errors.BulkAddNoPossible + " " + ex.InnerException);
            }
            return false;
        }

        private bool IsNullSelectedBulkCompartment()
        {
            if (this.selectedBulkCompartmentTray != null && this.selectedBulkCompartmentTray.Width != 0 && this.selectedBulkCompartmentTray.Height != 0 &&
                this.selectedBulkCompartmentTray.Row != 0 && this.selectedBulkCompartmentTray.Column != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OnSelectedBulkCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CanExecuteBulkAddCommand();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
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
