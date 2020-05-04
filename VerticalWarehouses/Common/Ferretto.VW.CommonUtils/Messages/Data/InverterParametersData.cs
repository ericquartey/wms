using System.Collections.Generic;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterParametersData
    {
        #region Constructors

        public InverterParametersData(byte inverterIndex, string description, IEnumerable<object> parameters)
        {
            this.InverterIndex = inverterIndex;
            this.Description = description;
            this.Parameters = parameters;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public byte InverterIndex { get; }

        public IEnumerable<object> Parameters { get; }

        #endregion
    }
}
