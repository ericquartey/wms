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
        private bool showBackground;

        private Tray tray;

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

        public bool ShowBackground
        {
            get => this.showBackground;
            set => this.SetProperty(ref this.showBackground, value);
        }

        public Tray Tray { get => this.tray; set => this.SetProperty(ref this.tray, value); }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            //Initialize without Origin, default: BOTTOM-LEFT
            this.tray = new Tray { Dimension = new Dimension { Height = 500, Width = 1960 } };//, Origin = new Position { XPosition = 0, YPosition = 0 } };

            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0, Code = "1", Id = 1 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0, Code = "2", Id = 2 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 0, YPosition = 0, Code = "3", Id = 3 });
            this.tray.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1760, YPosition = 300, Code = "4", Id = 4 });
            this.RaisePropertyChanged(nameof(this.Tray));
            this.RaisePropertyChanged(nameof(this.Tray.Compartments));

            this.InitializeInput();
        }

        private void CompatmentSelected_UpdateCompartmentEvent(Object sender, EventArgs e)
        {
            this.CompartmentSelected = (CompartmentDetails)sender;
        }

        private void ExecuteNewCreateCompartmentCommand()
        {
            var compartmentDetails = new CompartmentDetails
            {
                Width = this.CompartmentSelected.Width,
                Height = this.CompartmentSelected.Height,
                XPosition = this.CompartmentSelected.XPosition,
                YPosition = this.CompartmentSelected.YPosition
            };
            this.Tray.AddCompartment(compartmentDetails);
        }

        private void InitializeInput()
        {
            this.CompartmentSelected = new CompartmentDetails
            {
                Width = 1,
                Height = 1,
                XPosition = 0,
                YPosition = 0,
                Stock = 0,
                ItemCode = ""
            };
        }

        #endregion Methods
    }
}
