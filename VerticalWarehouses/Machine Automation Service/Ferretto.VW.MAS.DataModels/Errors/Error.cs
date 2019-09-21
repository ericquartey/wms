using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Error : DataModel
    {


        #region Properties

        public BayNumber BayNumber { get; set; }

        public int Code { get; set; }

        public ErrorDefinition Definition { get; set; }

        public DateTime OccurrenceDate { get; set; }

        public DateTime? ResolutionDate { get; set; }

        #endregion
    }
}
