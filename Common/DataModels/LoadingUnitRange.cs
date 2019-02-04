namespace Ferretto.Common.DataModels
{
    // Range Udc
    public sealed class LoadingUnitRange : IDataModel
    {
        #region Properties

        public int? ActualValue { get; set; }

        public Area Area { get; set; }

        public int AreaId { get; set; }

        public int Id { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        #endregion
    }
}
