using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class ErrorDefinition
    {
        #region Properties

        public int Code { get; set; }

        public string Description { get; set; }

        public IEnumerable<Error> Occurrences { get; set; }

        public string Reason { get; set; }

        public int Severity { get; set; }

        public ErrorStatistic Statistics { get; set; }

        #endregion
    }
}
