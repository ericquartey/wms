using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class FieldMessageData : IFieldMessageData
    {
        protected FieldMessageData(MessageVerbosity verbosity)
        {
            this.Verbosity = verbosity;
        }

        public MessageVerbosity Verbosity { get; }
    }
}
