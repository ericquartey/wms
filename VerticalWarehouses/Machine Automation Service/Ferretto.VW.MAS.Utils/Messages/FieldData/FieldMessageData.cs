using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class FieldMessageData : IFieldMessageData
    {
        #region Constructors

        protected FieldMessageData(MessageVerbosity verbosity)
        {
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
