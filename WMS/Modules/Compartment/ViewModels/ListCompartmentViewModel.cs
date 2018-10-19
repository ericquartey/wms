using System;
using System.Collections.Generic;
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

        private IList<CompartmentDetails> compartmentsProperty;

        #endregion Fields

        #region Properties

        public IList<CompartmentDetails> CompartmentsProperty
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

        public void UpdateGridList(IList<CompartmentDetails> compartments)
        {
            this.CompartmentsProperty = compartments;
        }

        protected override void OnAppear()
        {
            base.OnAppear();
            //this.TestInitializeGrid();
        }

        private void TestInitializeGrid()
        {
            this.CompartmentsProperty = new List<CompartmentDetails>()
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
            this.RaisePropertyChanged(nameof(this.CompartmentsProperty));
        }

        #endregion Methods
    }
}
