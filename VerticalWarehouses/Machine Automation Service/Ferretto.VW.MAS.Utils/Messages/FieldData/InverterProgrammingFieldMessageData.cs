using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterProgrammingFieldMessageData : FieldMessageData, IInverterProgrammingFieldMessageData
    {
        #region Constructors

        public InverterProgrammingFieldMessageData(
            IEnumerable<object> parameters,
            bool isCheckInverterVersion,
            byte inverterIndex,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (parameters is null)
            {
                throw new System.ArgumentNullException(nameof(parameters));
            }

            this.Parameters = parameters;
            this.IsCheckInverterVersion = isCheckInverterVersion;
            this.InverterIndex = inverterIndex;
        }

        #endregion

        #region Properties

        public byte InverterIndex { get; }

        public bool IsCheckInverterVersion { get; }

        public IEnumerable<object> Parameters { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (this.Parameters?.Count() == 0)
            {
                return "No parameters found";
            }
            return $"Parameters count {this.Parameters?.Count()}";
        }

        #endregion
    }
}
