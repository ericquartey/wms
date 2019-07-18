using System.Collections.Generic;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlDrawerWeightSaturationDataGridViewModel : BaseViewModel, ICustomControlDrawerWeightSaturationDataGridViewModel
    {
        #region Fields

        private IEnumerable<LoadingUnitWeightStatistics> loadingUnits;

        private LoadingUnitWeightStatistics selectedLoadingUnit;

        #endregion

        #region Properties

        public IEnumerable<LoadingUnitWeightStatistics> LoadingUnits { get => this.loadingUnits; set => this.SetProperty(ref this.loadingUnits, value); }

        public LoadingUnitWeightStatistics SelectedLoadingUnit { get => this.selectedLoadingUnit; set => this.SetProperty(ref this.selectedLoadingUnit, value); }

        #endregion
    }
}
