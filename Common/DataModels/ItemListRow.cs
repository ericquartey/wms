using System;
using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Riga di Lista Articoli
    public sealed class ItemListRow : ITimestamped, IDataModel
    {
        #region Properties

        public string Code { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime CreationDate { get; set; }

        public int EvadedQuantity { get; set; }

        public int Id { get; set; }

        public Item Item { get; set; }

        public int ItemId { get; set; }

        public ItemList ItemList { get; set; }

        public int ItemListId { get; set; }

        public DateTime? LastExecutionDate { get; set; }

        public DateTime LastModificationDate { get; set; }

        public string Lot { get; set; }

        public MaterialStatus MaterialStatus { get; set; }

        public int MaterialStatusId { get; set; }

        public List<Mission> Missions { get; set; }

        public PackageType PackageType { get; set; }

        public int PackageTypeId { get; set; }

        public int Priority { get; set; }

        public string RegistrationNumber { get; set; }

        public int RequiredQuantity { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        public ItemListRowStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        #endregion Properties
    }
}
