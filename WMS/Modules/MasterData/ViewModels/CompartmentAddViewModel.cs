using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentAddViewModel : SidePanelDetailsViewModel<CompartmentEdit>
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private CompartmentDetails compartment;
        private bool enableInputAdd;

        private IDataSource<Item> itemsDataSource;
        private int loadingUnitId;

        private Tray tray;

        #endregion Fields

        #region Constructors

        public CompartmentAddViewModel()
        {
            this.Title = Common.Resources.MasterData.AddCompartment;
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails Compartment
        {
            get => this.compartment;
            set => this.SetProperty(ref this.compartment, value);
        }

        public bool EnableCheck { get; set; }

        public bool EnableInputAdd
        {
            get => this.enableInputAdd;
            set => this.SetProperty(ref this.enableInputAdd, value);
        }

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

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

        public override void RefreshData()
        {
            throw new NotImplementedException();
        }

        protected override bool CanExecuteSaveCommand()
        {
            if (!this.EnableCheck && this.IsNullSelectedCompartment())
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
            // this.SetError();
            this.EnableCheck = true;
            if (await this.SaveLoadingUnit())
            {
                this.ResetView();
            }
        }

        private bool CanExecuteAddCommand()
        {
            if (this.EnableCheck)
            {
                var error = this.Tray.CanBulkAddCompartment(this.Compartment, this.Tray, true);
                //   this.SetError(error);
                return error != null && error.Trim() != "";
            }
            return true;
        }

        private void ExecuteCancelCommand()
        {
            this.ResetView();
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
            this.EnableCheck = false;
        }

        private void ResetView()
        {
            this.EnableInputAdd = false;
            this.Compartment.PropertyChanged -= this.OnSelectedCompartmentPropertyChanged;
            this.Reset();
            this.CompleteOperation();
        }

        private async Task<bool> SaveLoadingUnit()
        {
            if (this.tray.CanAddCompartment(this.Compartment))
            {
                this.Compartment.LoadingUnitId = this.loadingUnitId;

                var result = await this.compartmentProvider.Add(this.Compartment);
                if (result.Success)
                {
                    this.tray.AddCompartment(this.Compartment);
                }
                else
                {
                    // this.SetError(result.Description);
                }
                return result.Success;
            }
            else
            {
                //  this.SetError(Errors.AddNoPossible);
            }

            return false;
        }

        #endregion Methods
    }
}
