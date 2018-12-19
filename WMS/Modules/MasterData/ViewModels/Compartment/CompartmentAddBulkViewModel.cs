using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddBulkViewModel : SidePanelDetailsViewModel<CompartmentDetails>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private bool enableInputBulkAdd;

        private BulkCompartment selectedBulkCompartmentTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentAddBulkViewModel()
        {
            this.Title = Common.Resources.MasterData.BulkAddCompartment;
        }

        #endregion Constructors

        #region Properties

        public bool EnableCheck { get; set; }

        public bool EnableInputBulkAdd
        {
            get => this.enableInputBulkAdd;
            set => this.SetProperty(ref this.enableInputBulkAdd, value);
        }

        public BulkCompartment SelectedBulkCompartmentTray
        {
            get => this.selectedBulkCompartmentTray;
            set => this.SetProperty(ref this.selectedBulkCompartmentTray, value);
        }

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

        protected override bool CanExecuteSaveCommand()
        {
            if (!this.EnableCheck && this.IsNullSelectedBulkCompartment())
            {
                return false;
            }
            return true;
        }

        protected override Task ExecuteRevertCommand()
        {
            throw new NotImplementedException();
        }

        protected override async void ExecuteSaveCommand()
        {
            this.EnableCheck = true;
            if (await this.GenerateBulkCompartments())
            {
                this.ResetView();
            }
        }

        private bool CanExecuteBulkAddCommand()
        {
            if (this.EnableCheck)
            {
                var error = this.Tray.CanBulkAddCompartment(this.SelectedBulkCompartmentTray, this.Tray, true);

                return error == null || error.Trim() == "";
            }
            return true;
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
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
                    // HACK: this needs to be removed // compartment.CompartmentTypeId = 2;
                    var result = await this.compartmentProvider.Add(compartment as CompartmentDetails);
                    if (!result.Success)
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
                // this.SetError(Errors.BulkAddNoPossible + " " + ex.InnerException);
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
            return true;
        }

        private void OnSelectedBulkCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CanExecuteBulkAddCommand();

            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        private void Reset()
        {
            this.EnableCheck = false;
            this.EnableInputBulkAdd = false;
        }

        private void ResetView()
        {
            this.EnableInputBulkAdd = false;
            this.SelectedBulkCompartmentTray.PropertyChanged -= this.OnSelectedBulkCompartmentPropertyChanged;
            this.Reset();
            this.CompleteOperation();
        }

        #endregion Methods
    }
}
