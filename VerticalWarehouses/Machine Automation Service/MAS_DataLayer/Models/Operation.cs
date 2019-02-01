using System.Collections.Generic;

namespace MAS_DataLayer
{
    public class Operation
    {
        #region Properties

        public string Name { get; set; }

        public int OperationId { get; set; }

        public ICollection<Step> Steps { get; set; }

        #endregion Properties
    }
}
