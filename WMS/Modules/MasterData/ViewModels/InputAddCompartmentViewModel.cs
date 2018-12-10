using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class InputAddCompartmentViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private ICommand cancelCommand;
        private CompartmentDetails compartment;
        private bool enableInputAdd;
        private string error;
        private string errorColor;
        private IDataSource<Item> itemsDataSource;
        private int loadingUnitId;
        private ICommand saveCommand;
        private string title = Common.Resources.MasterData.AddCompartment;
        private Tray tray;

        #endregion Fields

        #region Events

        public event EventHandler FinishEvent;

        #endregion Events

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

        public CompartmentDetails Compartment
        {
            get => this.compartment;
            set => this.SetProperty(ref this.compartment, value);
        }

        public bool EnableCheck { get; set; }

        public bool EnableInputAdd
        {
            get { return this.enableInputAdd; }
            set { this.SetProperty(ref this.enableInputAdd, value); }
        }

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public string ErrorColor { get => this.errorColor; set => this.SetProperty(ref this.errorColor, value); }

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                                 (this.saveCommand = new DelegateCommand(() => this.ExecuteSaveCommand().ConfigureAwait(false), this.CanExecuteSaveCommand)
                                 .ObservesProperty(() => this.Error)
                                .ObservesProperty(() => this.Compartment));

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
            this.EnableInputAdd = true;
            this.loadingUnitId = loadingUnitId;
            this.InitializeData();
        }

        protected virtual void OnFinishEvent(EventArgs e)
        {
            EventHandler handler = this.FinishEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool CanExecuteAddCommand()
        {
            if (this.EnableCheck)
            {
                string error = this.Tray.CanBulkAddCompartment(this.Compartment, this.Tray, true);
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

        private void ExecuteCancelCommand()
        {
            this.ResetView();
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

        private void InitializeData()
        {
            this.Compartment = this.compartmentProvider.GetNewCompartmentDetails();
            this.Compartment.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;

            this.ItemsDataSource = new DataSource<Item>(() => this.itemProvider.GetAll());
        }

        private bool IsNullSelectedCompartment()
        {
            if (this.compartment != null && this.compartment.XPosition != null && this.compartment.YPosition != null
                && this.compartment.Width != null && this.compartment.Height != null && this.compartment.Width != 0 && this.compartment.Height != 0)
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
            this.CanExecuteAddCommand();
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
            this.Compartment.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.Reset();
            this.OnFinishEvent(null);
        }

        private async Task<bool> SaveLoadingUnit()
        {
            if (this.tray.CanAddCompartment(this.Compartment))
            {
                this.Compartment.LoadingUnitId = this.loadingUnitId;
                //TODO: implement create new Compartment Type
                this.Compartment.CompartmentTypeId = 2;

                var add = await this.compartmentProvider.Add(this.Compartment);
                if (add == 1)
                {
                    this.tray.AddCompartment(this.Compartment);
                }
                return true;
            }
            else
            {
                this.SetError(Errors.AddNoPossible);
            }

            return false;
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
