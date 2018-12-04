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
                          (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

        public ICommand DeleteCommand => this.deleteCommand ??
                  (this.deleteCommand = new DelegateCommand(this.ExecuteDeleteCommand, this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedCompartmentTray));

        public bool EnableInputEdit
        {
            get => this.enableInputEdit;
            set => this.SetProperty(ref this.enableInputEdit, value);
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

        #region Methods

        public void Initialize(Tray tray, LoadingUnitDetails loadingUnit, CompartmentDetails selectedCompartmentTray)
        {
            this.tray = tray;
            this.EnableInputEdit = true;
            this.SelectedCompartmentTray = selectedCompartmentTray;
            this.SelectedCompartmentTray.PropertyChanged += this.OnSelectedCompartmentPropertyChanged;
            this.loadingUnit = loadingUnit;
        }

        protected virtual void OnFinishEvent(EventArgs e)
        {
            EventHandler handler = this.FinishEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedCompartmentTray != null;
        }

        private bool CanExecuteEditCommand()
        {
            string error = this.tray.CanBulkAddCompartment(this.SelectedCompartmentTray, this.tray, true, true);
            this.SetError(error);
            if (error == null || error == "")
            {
                return true;
            }
            return false;
        }

        private bool CanExecuteSaveCommand()
        {
            return this.Error == null || this.Error.Trim() == "";
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

        private void ExecuteSaveCommand()
        {
            this.SetError();
            if (this.SaveLoadingUnit())
            {
                this.ResetView();
            }
        }

        private void OnSelectedCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CanExecuteEditCommand();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        private void Reset()
        {
            this.SetError();
        }

        private void ResetView()
        {
            this.EnableInputEdit = false;
            this.SelectedCompartmentTray.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.Reset();
            this.OnFinishEvent(null);
        }

        private bool SaveLoadingUnit()
        {
            if (this.tray.CanAddCompartment(this.SelectedCompartmentTray, true))
            {
                var compartment = this.loadingUnit.Compartments.Single(c => c.Id == this.SelectedCompartmentTray.Id);
                this.UpdateCompartment(compartment);
                var modifiedRowCount = this.loadingUnitProvider.Save(this.loadingUnit);

                if (modifiedRowCount > 0)
                {
                    this.EventService.Invoke(new ModelChangedEvent<LoadingUnit>(this.loadingUnit.Id));

                    this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
                }
                else
                {
                    this.SetError(Errors.NoChangesFound);
                    return false;
                }
            }
            else
            {
                this.SetError(Errors.EditNoPossible);
                return false;
            }
            this.tray.Update(this.SelectedCompartmentTray);
            return true;
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

        private void UpdateCompartment(CompartmentDetails compartment)
        {
            compartment.XPosition = this.SelectedCompartmentTray.XPosition;
            compartment.YPosition = this.SelectedCompartmentTray.YPosition;
            compartment.Width = this.SelectedCompartmentTray.Width;
            compartment.Height = this.SelectedCompartmentTray.Height;
            compartment.ItemCode = this.SelectedCompartmentTray.ItemCode;
            compartment.Stock = this.SelectedCompartmentTray.Stock;
            compartment.MaxCapacity = this.SelectedCompartmentTray.MaxCapacity;
            compartment.ItemPairing = this.SelectedCompartmentTray.ItemPairing;
        }

        #endregion Methods
    }
}
