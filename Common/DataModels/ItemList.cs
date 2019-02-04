using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Lista Articoli
    public sealed class ItemList : ITimestamped, IDataModel
    {
        #region Properties

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public string CustomerOrderCode { get; set; }

        public string CustomerOrderDescription { get; set; }

        public string Description { get; set; }

        public DateTime? ExecutionEndDate { get; set; }

        public DateTime? FirstExecutionDate { get; set; }

        public int Id { get; set; }

        public IEnumerable<ItemListRow> ItemListRows { get; set; }

        public ItemListType ItemListType { get; set; }

        public string Job { get; set; }

        public DateTime LastModificationDate { get; set; }

        public IEnumerable<Mission> Missions { get; set; }

        public int Priority { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        public bool ShipmentUnitAssociated { get; set; }

        public string ShipmentUnitCode { get; set; }

        public string ShipmentUnitDescription { get; set; }

        public ItemListStatus Status { get; set; }

        #endregion
    }
}
