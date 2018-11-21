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
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class InputEditCompartmentViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        public Tray tray;
        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private ICommand cancelCommand;
        private bool createMode;
        private ICommand deleteCommand;
        private bool enableInputEdit;
        private string error;
        private string errorColor;
        private LoadingUnitDetails loadingUnit;
        private ICommand saveCommand;
        private CompartmentDetails selectedCompartmentTray;
        private string title = Common.Resources.MasterData.EditCompartment;

        #endregion Fields

        #region Constructors

        public InputEditCompartmentViewModel()
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

        public ICommand DeleteCommand => this.deleteCommand ??
                  (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedCompartmentTray));

        //public bool EnableCheck { get; set; }

        public bool EnableInputEdit
        {
            get { return this.enableInputEdit; }
            set { this.SetProperty(ref this.enableInputEdit, value); }
        }

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }
        public string ErrorColor { get => this.errorColor; set => this.SetProperty(ref this.errorColor, value); }

        public ICommand SaveCommand => this.saveCommand ??
                                 (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSaveCommand)
           .ObservesProperty(() => this.Error));

        public CompartmentDetails SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set => this.SetProperty(ref this.selectedCompartmentTray, value);
        }

        public string Title { get => this.title; private set => this.SetProperty(ref this.title, value); }

        #endregion Properties

        //public Tray Tray
        //{
        //    get => this.tray;
        //    set => this.SetProperty(ref this.tray, value);
        //}

        #region Methods

        public void Initialize(Tray tray, LoadingUnitDetails loadingUnit, CompartmentDetails selectedCompartmentTray)
        {
            this.tray = tray;
            //this.SelectedCompartmentTray = new CompartmentDetails();
            this.EnableInputEdit = true;
            this.SelectedCompartmentTray = selectedCompartmentTray;
            this.SelectedCompartmentTray.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;
            this.loadingUnit = loadingUnit;
        }

        protected virtual void OnFinishEvent(EventArgs e)
        {
            EventHandler handler = FinishEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool CanExecuteCancelCommand()
        {
            return true;
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null && !this.CreateMode;
        }

        private bool CanExecuteEditCommand()
        {
            //if (this.EnableCheck)
            //{
            string error = this.tray.CanBulkAddCompartment(this.SelectedCompartmentTray, this.tray, true, true);
            this.SetError(error);
            //}
            bool goAhead = false;
            if (error == null || error == "")
            {
                goAhead = true;
            }
            return goAhead;// !this.CreateMode;
        }

        private bool CanExecuteSaveCommand()
        {
            //if (!this.EnableCheck)
            //{
            //    return true;
            //}
            //bool x = this.CreateMode && (this.Error == null || this.Error.Trim().Equals(""));
            return this.Error == null || this.Error.Trim() == "";// this.Error.Trim().Equals(""));
        }

        private void EnableCreation()
        {
            this.SetError();
            //this.SetSelectedCompartment(new CompartmentDetails());// { Width = 0, Height = 0, XPosition = 0, YPosition = 0 });
            //this.SelectedCompartment = null;
            //this.CreateMode = true;
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
        }

        private void ExecuteDeleteCommand()
        {
            this.SetError();
            this.tray.Compartments.Remove(this.SelectedCompartmentTray);
            this.compartmentProvider.Delete(this.SelectedCompartmentTray.Id);
            this.ResetView();
        }

        private void ExecuteEditCommand()
        {
            this.EnableCreation();
        }

        private void ExecuteFinishCommand()
        {
            //TO PARENT UPDATE
        }

        private void ExecuteSaveCommand()
        {
            this.SetError();
            //this.EnableCheck = true;
            if (this.SaveLoadingUnit())
            {
                this.ResetView();
            }
        }

        private void OnSelectedCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.CanExecuteEditCommand())
            {
                this.ExecuteEditCommand();
            }
            this.CanExecuteSaveCommand();
        }

        private void Reset()
        {
            this.SetError();
            //this.EnableCheck = false;
        }

        private void ResetView()
        {
            //this.CreateMode = false;
            this.EnableInputEdit = false;
            this.SelectedCompartmentTray.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.Reset();
            this.OnFinishEvent(null);
        }

        private bool SaveLoadingUnit()
        {
            bool ok = false;
            if (this.tray.CanAddCompartment(this.SelectedCompartmentTray, true))
            {
                var modifiedRowCount = this.loadingUnitProvider.Save(this.loadingUnit);

                if (modifiedRowCount > 0)
                {
                    this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.loadingUnit.Id));

                    this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
                }
                else
                {
                    //this.SetError("Errors");
                }
            }
            ok = true;
            //else
            //{
            //    this.SetError("Errors");
            //}
            this.tray.Update(this.SelectedCompartmentTray);
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

        private void Tray_CompartmentChangedEvent(Object sender, Tray.CompartmentEventArgs e)
        {
        }

        #endregion Methods
    }
}
