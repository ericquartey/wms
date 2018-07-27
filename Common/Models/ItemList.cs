using System;
using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Lista Articoli
    public partial class ItemList
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int ItemListTypeId { get; set; }
        public string Description { get; set; }
        public int AreaId { get; set; }
        public int Priority { get; set; }
        public int ItemListStatusId { get; set; }
        public bool ShipmentUnitAssociated { get; set; }
        public string ShipmentUnitCode { get; set; }
        public string ShipmentUnitDescription { get; set; }
        public string Job { get; set; }
        public string CustomerOrderCode { get; set; }
        public string CustomerOrderDescription { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public DateTime? FirstExecutionDate { get; set; }
        public DateTime? ExecutionEndDate { get; set; }

        public ItemListType ItemListType { get; set; }
        public ItemListStatus ItemListStatus { get; set; }
        public Area Area { get; set; }

        public List<ItemListRow> ItemListRows { get; set; }
    }
}
