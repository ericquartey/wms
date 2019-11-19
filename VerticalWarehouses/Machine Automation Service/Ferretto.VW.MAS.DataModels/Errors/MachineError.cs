using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MachineError : DataModel
    {
        #region Properties

        public BayNumber BayNumber { get; set; }

        public int Code { get; set; }

        public ErrorDefinition Definition { get; set; }

        /// <summary>
        /// The occurrence date, in local time.
        /// </summary>
        public DateTime OccurrenceDate { get; set; }

        /// <summary>
        /// The resolution date, in local time.
        /// </summary>
        public DateTime? ResolutionDate { get; set; }

        #endregion
    }
}
