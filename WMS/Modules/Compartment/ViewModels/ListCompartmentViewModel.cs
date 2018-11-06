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

        private ObservableCollection<WmsBaseCompartment> compartmentsProperty;
        private CompartmentDetails selectedcompartment;

        #endregion Fields

        #region Properties

        public ObservableCollection<WmsBaseCompartment> CompartmentsProperty
        {
            get { return this.compartmentsProperty; }
            set
            {
                this.compartmentsProperty = value;
                this.RaisePropertyChanged(nameof(this.CompartmentsProperty));
            }
        }

        public CompartmentDetails SelectedCompartment
        {
            get { return this.selectedcompartment; }
            set
            {
                this.selectedcompartment = value;
                this.RaisePropertyChanged(nameof(this.SelectedCompartment));
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
