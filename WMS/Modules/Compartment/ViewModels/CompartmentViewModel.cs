using System.Collections.Generic;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.Compartment
{
    public class CompartmentViewModel : BaseNavigationViewModel
    {
        #region Fields

        private LoadingUnitDetails loadingUnitDetails;

        #endregion Fields

        #region Constructors

        public CompartmentViewModel()
        {
            this.loadingUnitDetails = new LoadingUnitDetails { Width = 1950, Length = 650 };
            var listCompartmentDetails = new List<CompartmentDetails>();
            //for (int i = 0; i < 5; i++)
            //{
            //    listCompartmentDetails.Add(new CompartmentDetails { Width = 150, Height = 150, XPosition = i + 10, YPosition = i + 10 });
            //}
            listCompartmentDetails.Add(new CompartmentDetails { Width = 150, Height = 150, XPosition = 0, YPosition = 0 });

            this.loadingUnitDetails.Compartments = listCompartmentDetails;
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set { this.SetProperty(ref this.loadingUnitDetails, value); } }

        #endregion Properties
    }
}
