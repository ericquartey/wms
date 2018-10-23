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
            this.loadingUnitDetails = new LoadingUnitDetails();// { Width = 1960, Length = 500 };
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 800, YPosition = 0, Code = "1", Id = 1 });
            this.loadingUnitDetails.AddCompartment(new CompartmentDetails() { Width = 200, Height = 200, XPosition = 1000, YPosition = 0, Code = "2", Id = 2 });
            //this.RaisePropertyChanged(nameof(this.LoadingUnit));
            this.RaisePropertyChanged(nameof(this.LoadingUnit.Compartments));
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
            this.LoadingUnit.AddCompartment(compartmentDetails);
        }

        #endregion Methods
    }
}
