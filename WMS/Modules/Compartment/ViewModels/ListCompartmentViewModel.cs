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

        //public void TestInitializeGrid()
        //{
        //    this.CompartmentsProperty = new ObservableCollection<WmsBaseCompartment>()
        //    {
        //        new CompartmentDetails()
        //        {
        //            Code = "1",
        //            XPosition = 0,
        //            YPosition = 0,
        //            Width = 150,
        //            Height = 150
        //        },
        //        new CompartmentDetails()
        //        {
        //            Code = "2",
        //            XPosition = 150,
        //            YPosition = 150,
        //            Width = 150,
        //            Height = 150
        //        },
        //        new CompartmentDetails()
        //        {
        //            Code = "3",
        //            XPosition = 150,
        //            YPosition = 150,
        //            Width = 150,
        //            Height = 150
        //        },
        //        new CompartmentDetails()
        //        {
        //            Code = "4",
        //            XPosition = 150,
        //            YPosition = 150,
        //            Width = 150,
        //            Height = 150
        //        }
        //    };
        //    this.RaisePropertyChanged(nameof(this.CompartmentsProperty));
        //    //this.CompartmentsProperty = compartmentsProperty;
        //}

        #region Methods

        public void UpdateGridList(ObservableCollection<WmsBaseCompartment> compartments)
        {
            this.CompartmentsProperty = compartments;

            //this.RaisePropertyChanged(nameof(this.CompartmentsProperty));
        }

        protected override void OnAppear()
        {
            base.OnAppear();
            //this.TestInitializeGrid();
        }

        #endregion Methods
    }
}
