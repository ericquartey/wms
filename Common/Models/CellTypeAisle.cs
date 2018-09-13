namespace Ferretto.Common.Models
{
    // Tipo Cella-Corridoio
    public sealed class CellTypeAisle
    {
        public int Id { get; set; }
        public int AisleId { get; set; }
        public int CellTypeId { get; set; }
        public int CellTypeTotal { get; set; }
        public decimal Ratio { get; set; }

        public Aisle Aisle { get; set; }
        public CellType CellType { get; set; }
    }
}
