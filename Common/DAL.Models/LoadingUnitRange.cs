namespace Ferretto.Common.DAL.Models
{
    // Range Udc
    public sealed class LoadingUnitRange
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? ActualValue { get; set; }

        public Area Area { get; set; }
    }
}
