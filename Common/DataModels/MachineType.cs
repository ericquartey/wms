using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Macchina
    public sealed class MachineType
    {
        #region Properties

        public string Description { get; set; }

        public string Id { get; set; }

        public List<Machine> Machines { get; }

        #endregion Properties
    }
}
