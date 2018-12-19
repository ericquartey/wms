using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Scomparto
    public sealed class CompartmentStatus : IDataModel
    {
        #region Properties

        public IEnumerable<Compartment> Compartments { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        #endregion Properties
    }
}
