using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Error
    {


        #region Properties

        public BayNumber BayIndex { get; set; }

        public int Code { get; set; }

        public ErrorDefinition Definition { get; set; }

        public int Id { get; set; }

        public DateTime OccurrenceDate { get; set; }

        public DateTime? ResolutionDate { get; set; }

        #endregion
    }
}
