using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

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
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentInput
        {
            get
            {
                return this.compartmentInput;
            }
            set
            {
                this.compartmentInput = value;
                this.RaisePropertyChanged(nameof(this.compartmentInput));
            }
        }

        public ICommand CreateNewCompartmentCommand => this.createNewCompartmentCommand ??
                 (this.createNewCompartmentCommand = new DelegateCommand(this.ExecuteNewCreateCompartmentCommand));

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set { this.SetProperty(ref this.loadingUnitDetails, value); } }

        #endregion Properties

        #region Methods

        public void UpdateTray(LoadingUnitDetails loadingUnitDetails)
        {
            this.LoadingUnit = loadingUnitDetails;
        }

        protected override void OnAppear()
        {
            this.loadingUnitDetails = new LoadingUnitDetails { Width = 1960, Length = 500 };
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
        }

        private void ExecuteNewCreateCompartmentCommand()
        {
            CompartmentDetails compartmentDetails = new CompartmentDetails
            {
                Width = this.CompartmentInput.Width,
                Height = this.CompartmentInput.Height,
                XPosition = this.CompartmentInput.XPosition,
                YPosition = this.CompartmentInput.YPosition
            };
            this.LoadingUnit.AddCompartment(compartmentDetails);

            this.LoadingUnit.OnAddedCompartmentEvent(null);
        }

        #endregion Methods
    }
}
