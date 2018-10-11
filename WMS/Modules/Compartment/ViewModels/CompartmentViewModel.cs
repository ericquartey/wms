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
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit { get => this.loadingUnitDetails; set { this.SetProperty(ref this.loadingUnitDetails, value); } }

        //public ICommand

        #endregion Properties
    }
}
