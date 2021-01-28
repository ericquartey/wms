using System.Collections.Generic;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterParametersData
    {
        #region Constructors

        public InverterParametersData()
        {
        }

        public InverterParametersData(InverterParametersData inverterParametersData)
        {
            this.Description = inverterParametersData.Description;
            this.InverterIndex = inverterParametersData.InverterIndex;
            this.IsCheckInverterVersion = inverterParametersData.IsCheckInverterVersion;
            this.Parameters = inverterParametersData.Parameters;
        }

        public InverterParametersData(byte inverterIndex, string description)
        {
            this.InverterIndex = inverterIndex;
            this.Description = description;
        }

        public InverterParametersData(byte inverterIndex, string description, IEnumerable<object> parameters, bool isCheckInverterVersion = false)
            : this(inverterIndex, description)
        {
            this.Parameters = parameters;
            this.IsCheckInverterVersion = isCheckInverterVersion;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public byte InverterIndex { get; }

        public bool IsCheckInverterVersion { get; }

        public IEnumerable<object> Parameters { get; }

        #endregion
    }
}
