using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Extensions;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class MachineError : DataModel
    {
        #region Properties

        public BayNumber BayNumber { get; set; }

        public int Code { get; set; }

        public string Description => ((MachineErrorCode)this.Code).GetDescription();

        /// <summary>
        /// The occurrence date, in local time.
        /// </summary>
        public DateTime OccurrenceDate { get; set; }

        public string Reason => ((MachineErrorCode)this.Code).GetReason();

        /// <summary>
        /// The resolution date, in local time.
        /// </summary>
        public DateTime? ResolutionDate { get; set; }

        #endregion
    }
}
