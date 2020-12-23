using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterReadingFieldMessageData : FieldMessageData, IInverterReadingFieldMessageData
    {
        #region Constructors

        public InverterReadingFieldMessageData(
            IEnumerable<object> parameters,
            bool isCheckInverterVersion,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (parameters is null)
            {
                throw new System.ArgumentNullException(nameof(parameters));
            }

            this.Parameters = parameters;
            this.IsCheckInverterVersion = isCheckInverterVersion;
        }

        #endregion

        #region Properties

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
