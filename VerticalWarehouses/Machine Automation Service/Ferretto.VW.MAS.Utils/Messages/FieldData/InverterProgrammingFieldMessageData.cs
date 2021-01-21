using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterProgrammingFieldMessageData : FieldMessageData, IInverterProgrammingFieldMessageData
    {
        #region Constructors

        public InverterProgrammingFieldMessageData(
            InverterParametersData inverterParametersData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (inverterParametersData is null)
            {
                throw new System.ArgumentNullException(nameof(inverterParametersData));
            }

            this.InverterParametersData = inverterParametersData;
        }

        #endregion

        #region Properties

        public InverterParametersData InverterParametersData { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (this.InverterParametersData?.Parameters?.Count() == 0)
            {
                return "No parameters found";
            }
            return $"Parameters count {this.InverterParametersData?.Parameters?.Count()}";
        }

        #endregion
    }
}
