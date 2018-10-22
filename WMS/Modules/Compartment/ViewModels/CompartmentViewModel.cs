using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private CompartmentDetails compartmentSelected;
        private ICommand createNewCompartmentCommand;
        private LoadingUnitDetails loadingUnitDetails;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentSelected
        {
            get => this.compartmentSelected;
            set
            {
                this.compartmentSelected = value;
                this.RaisePropertyChanged(nameof(this.CompartmentSelected));
            }
        }

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                 (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set => this.SetProperty(ref this.loadingUnitDetails, value); }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            this.loadingUnitDetails = new LoadingUnitDetails { Width = 1960, Length = 500 };
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0, Code = "1", Id=1 });
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0, Code = "2", Id=2 });
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
            this.RaisePropertyChanged(nameof(this.LoadingUnit.Compartments));

            this.TestInitializeInput();
            //this.TestInitializeGrid();

            //this.CompartmentSelected = new CompartmentDetails();
            //this.CompartmentSelected.UpdateCompartmentEvent += this.CompatmentSelected_UpdateCompartmentEvent;
        }

        //public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        //{
        //    this.LoadingUnit = loadingUnitDetails;
        //}
        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
            this.CompartmentSelected = (CompartmentDetails)sender;
        }

        //public LoadingUnitDetails CompartmentSelected { get => this.compa; set => this.SetProperty(ref this.loadingUnitDetails, value); }
        //public void UpdateInput(CompartmentDetails compartmentDetails)
        //{
        //    //TODO
        //    this.CompartmentSelected = compartmentDetails;
        //}
        private void ExecuteNewCreateCompartmentCommand()
        {
            var compartmentDetails = new CompartmentDetails
            {
                Width = this.CompartmentSelected.Width,
                Height = this.CompartmentSelected.Height,
                XPosition = this.CompartmentSelected.XPosition,
                YPosition = this.CompartmentSelected.YPosition
            };
            this.LoadingUnit.AddCompartment(compartmentDetails);
        }

        //private void TestInitializeGrid()
        //{
        //    this.compartments = new List<CompartmentDetails>()
        //    {
        //        new CompartmentDetails()
        //        {
        //            Code = "1",
        //            XPosition = 0,
        //            YPosition = 0,
        //            Width = 150,
        //            Height = 150
        //        }
        //    };
        //    this.Compartments = this.compartments;
        //}

        private void TestInitializeInput()
        {
            this.compartmentSelected = new CompartmentDetails();
            this.compartmentSelected.Width = 150;
            this.compartmentSelected.Height = 150;
            this.compartmentSelected.XPosition = 0;
            this.compartmentSelected.YPosition = 0;
            this.compartmentSelected.Stock = 0;
            this.compartmentSelected.ItemCode = "Item";

            this.CompartmentSelected = this.compartmentSelected;

            this.CompartmentSelected.UpdateCompartmentEvent += this.CompatmentSelected_UpdateCompartmentEvent;
        }

        #endregion Methods
    }
}
