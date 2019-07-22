using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    public class LoadingUnitEditViewData
    {
        #region Constructors

        public LoadingUnitEditViewData(int loadingUnitId, int? itemId, int? selectedCompartmentId)
        {
            this.LoadingUnitId = loadingUnitId;
            this.ItemId = itemId;
            this.SelectedCompartmentId = selectedCompartmentId;
        }

        #endregion

        #region Properties

        public int? ItemId { get; }

        public int LoadingUnitId { get; }

        public int? SelectedCompartmentId { get; set; }

        #endregion
    }
}
