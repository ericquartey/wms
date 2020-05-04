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
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (parameters is null)
            {
                throw new System.ArgumentNullException(nameof(parameters));
            }

            this.Parameters = parameters;
        }

        #endregion

        #region Properties

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
