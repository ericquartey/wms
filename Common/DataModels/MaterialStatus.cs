using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Materiale
    public sealed class MaterialStatus : IDataModel
    {
        #region Properties

        public IEnumerable<Compartment> Compartments { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public IEnumerable<ItemListRow> ItemListRows { get; set; }

        public IEnumerable<Mission> Missions { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        #endregion
    }
}
