using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class InverterNode
    {
        #region Constructors

        public InverterNode(byte inverterIndex, string description, IEnumerable<InverterNodeParameter> parameters)
        {
            this.Parameters = parameters;
            this.InverterIndex = inverterIndex;
            this.Description = description;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public byte InverterIndex { get; set; }

        public IEnumerable<InverterNodeParameter> Parameters { get; }

        #endregion
    }
}
