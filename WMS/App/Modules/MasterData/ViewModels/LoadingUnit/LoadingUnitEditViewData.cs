namespace Ferretto.WMS.Modules.MasterData
{
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
