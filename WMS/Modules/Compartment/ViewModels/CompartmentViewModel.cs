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
            this.loadingUnitDetails = new LoadingUnitDetails();
            this.loadingUnitDetails.Width = 1950;
            this.loadingUnitDetails.Length = 650;
            var lcd = new List<CompartmentDetails>();
            lcd.Add(new CompartmentDetails { Width = 150, Height = 150 });
            lcd.Add(new CompartmentDetails { Width = 150, Height = 150 });
            lcd.Add(new CompartmentDetails { Width = 150, Height = 150 });
            lcd.Add(new CompartmentDetails { Width = 150, Height = 150 });
            this.loadingUnitDetails.Compartments = lcd;

            //var listWmsCompartment = new List<WmsCompartment>();
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 0));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 150));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 300));
            //listWmsCompartment.Add(new WmsCompartment(150, 150, 0, 450));
            ////
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 0));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 150));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 300));
            //listWmsCompartment.Add(new WmsCompartment(50, 150, 150, 450));
            ////
            //listWmsCompartment.Add(new WmsCompartment(300, 300, 200, 0));
            //listWmsCompartment.Add(new WmsCompartment(300, 300, 200, 300));
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; }

        #endregion Properties
    }
}
