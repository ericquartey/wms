using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InstructionDefinition : DataModel
    {
        #region Properties

        public InstructionType InstructionType { get; set; }

        public string Description { get; set; }

        public string CounterName { get; set; }
        public int? MaxDays { get; set; }

        public int? MaxRelativeCount { get; set; }

        public int? MaxTotalCount { get; set; }

        public Axis Axis { get; set; }

        public BayNumber BayNumber { get; set; }

        public bool IsShutter { get; set; }
        public bool IsCarousel { get; set; }
        public bool IsSystem { get; set; }
        #endregion
    }
}
