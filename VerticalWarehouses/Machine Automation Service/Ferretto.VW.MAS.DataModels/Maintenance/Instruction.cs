using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Instruction : DataModel
    {
        #region Properties

        public InstructionDefinition Definition { get; set; }

        [Obsolete("Not used")]
        [JsonIgnore]
        public double? DoubleCounter { get; set; }

        public MachineServiceStatus InstructionStatus { get; set; } = MachineServiceStatus.Valid;

        [Obsolete("Not used")]
        [JsonIgnore]
        public int? IntCounter { get; set; }

        public bool IsDone { get; set; }

        public bool IsToDo { get; set; }

        [Obsolete("Not used")]
        [JsonIgnore]
        public DateTime? MaintenanceDate { get; set; }

        [JsonIgnore]
        public ServicingInfo ServicingInfo { get; set; }

        #endregion
    }
}
