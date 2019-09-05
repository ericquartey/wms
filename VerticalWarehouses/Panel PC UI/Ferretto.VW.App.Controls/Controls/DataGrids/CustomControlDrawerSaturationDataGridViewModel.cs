using System.Collections.Generic;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlDrawerSaturationDataGridViewModel : BaseViewModel, ICustomControlDrawerSaturationDataGridViewModel
    {
        #region Fields

        private IEnumerable<LoadingUnitSpaceStatistics> loadingUnits;

        private LoadingUnitSpaceStatistics selectedLoadingUnit;

        #endregion

        #region Properties

        public IEnumerable<LoadingUnitSpaceStatistics> LoadingUnits { get => this.loadingUnits; set => this.SetProperty(ref this.loadingUnits, value); }

        public LoadingUnitSpaceStatistics SelectedLoadingUnit { get => this.selectedLoadingUnit; set => this.SetProperty(ref this.selectedLoadingUnit, value); }

        #endregion
    }
}
