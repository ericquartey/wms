using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Compartment
{
    public class ListCompartmentViewModel : BaseNavigationViewModel
    {
        #region Fields

        private CompartmentDetails compartmentSelected;
        private ObservableCollection<WmsBaseCompartment> compartmentsProperty;

        #endregion Fields

        #region Properties

        public CompartmentDetails CompartmentSelected
        {
            get { return this.compartmentSelected; }
            set
            {
                this.compartmentSelected = value;
                this.RaisePropertyChanged(nameof(this.CompartmentSelected));
            }
        }

        public ObservableCollection<WmsBaseCompartment> CompartmentsProperty
        {
            get { return this.compartmentsProperty; }
            set
            {
                this.compartmentsProperty = value;
                this.RaisePropertyChanged(nameof(this.CompartmentsProperty));
            }
        }

        #endregion Properties

        #region Methods

        public void UpdateGridList(ObservableCollection<WmsBaseCompartment> compartments)
        {
            this.CompartmentsProperty = compartments;
        }

        protected override void OnAppear()
        {
            base.OnAppear();
        }

        #endregion Methods
    }
}
