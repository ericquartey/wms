using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Macchina
    public sealed class MachineType
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public List<Machine> Machines { get; set; }
    }
}
