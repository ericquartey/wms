using System;
using System.ComponentModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Compartment
{
    public class CompartmentViewModel : BaseNavigationViewModel
    {
        #region Fields

        private CompartmentDetails compartmentInput;
        private ICommand createNewCompartmentCommand;
        private LoadingUnitDetails loadingUnitDetails;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
            this.compartmentInput = new CompartmentDetails();
            this.compartmentInput.Width = 150;
            this.compartmentInput.Height = 150;
            this.compartmentInput.XPosition = 0;
            this.compartmentInput.YPosition = 0;
            this.compartmentInput.Stock = 0;
            this.compartmentInput.ItemCode = "Item";

            this.CompartmentInput = this.compartmentInput;

            this.CompartmentInput.UpdateCompartmentEvent += this.CompatmentSelected_UpdateCompartmentEvent;
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentInput
        {
            get => this.compartmentInput;
            set
            {
                this.compartmentInput = value;
                this.RaisePropertyChanged(nameof(this.compartmentInput));
            }
        }

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                 (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set => this.SetProperty(ref this.loadingUnitDetails, value); }

        #endregion Properties

        //public LoadingUnitDetails CompartmentSelected { get => this.compa; set => this.SetProperty(ref this.loadingUnitDetails, value); }
        //public void UpdateInput(CompartmentDetails compartmentDetails)
        //{
        //    //TODO
        //    this.CompartmentSelected = compartmentDetails;
        //}

        //public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        //{
        //    this.LoadingUnit = loadingUnitDetails;
        //}

        #region Methods

        protected override void OnAppear()
        {
            this.loadingUnitDetails = new LoadingUnitDetails { Width = 1960, Length = 500 };
            this.RaisePropertyChanged(nameof(this.LoadingUnit));

            //this.CompartmentSelected = new CompartmentDetails();
            //this.CompartmentSelected.UpdateCompartmentEvent += this.CompatmentSelected_UpdateCompartmentEvent;
        }

        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
            this.CompartmentInput = (CompartmentDetails)sender;
            //this.CompartmentInput;
            //throw new NotImplementedException();
        }

        private void ExecuteNewCreateCompartmentCommand()
        {
            var compartmentDetails = new CompartmentDetails
            {
                Width = this.CompartmentInput.Width,
                Height = this.CompartmentInput.Height,
                XPosition = this.CompartmentInput.XPosition,
                YPosition = this.CompartmentInput.YPosition
            };
            this.LoadingUnit.AddCompartment(compartmentDetails);
        }

        #endregion Methods
    }
}
