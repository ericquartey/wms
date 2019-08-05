using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class Error
    {
        #region Properties

        public int Code { get; set; }

        public ErrorDefinition Definition { get; set; }

        public int Id { get; set; }

        public DateTime OccurrenceDate { get; set; }

        public DateTime? ResolutionDate { get; set; }

        #endregion
    }
}
