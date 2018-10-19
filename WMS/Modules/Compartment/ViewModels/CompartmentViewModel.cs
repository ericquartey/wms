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

        private CompartmentDetails compartmentInput;
        private IList<CompartmentDetails> compartments;
        private ICommand createNewCompartmentCommand;
        private ObservableCollection<WmsBaseCompartment> items;
        private LoadingUnitDetails loadingUnitDetails;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentInput
        {
            get => this.compartmentInput;
            set
            {
                this.compartmentInput = value;
                this.RaisePropertyChanged(nameof(this.CompartmentInput));
            }
        }

        public IList<CompartmentDetails> Compartments
        {
            get { return this.compartments; }
            set
            {
                this.compartments = value;
                this.RaisePropertyChanged(nameof(this.Compartments));
            }
        }

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                 (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public ObservableCollection<WmsBaseCompartment> Items
        {
            get => this.items;
            set
            {
                this.items = value;
                this.RaisePropertyChanged(nameof(this.Items));
            }
        }

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set => this.SetProperty(ref this.loadingUnitDetails, value); }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            this.loadingUnitDetails = new LoadingUnitDetails { Width = 1960, Length = 500 };
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0 });
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0 });
            this.RaisePropertyChanged(nameof(this.LoadingUnit));

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
            this.CompartmentInput = (CompartmentDetails)sender;
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
                Width = this.CompartmentInput.Width,
                Height = this.CompartmentInput.Height,
                XPosition = this.CompartmentInput.XPosition,
                YPosition = this.CompartmentInput.YPosition
            };
            this.LoadingUnit.AddCompartment(compartmentDetails);
        }

        private void TestInitializeGrid()
        {
            this.compartments = new List<CompartmentDetails>()
            {
                new CompartmentDetails()
                {
                    Code = "1",
                    XPosition = 0,
                    YPosition = 0,
                    Width = 150,
                    Height = 150
                }
            };
            this.Compartments = this.compartments;
        }

        private void TestInitializeInput()
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

        #endregion Methods
    }
}
