using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class CurrentPositionFieldMessageData : IFieldMessageData
    {
        #region Constructors

        public CurrentPositionFieldMessageData(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        #endregion

        #region Properties

        public decimal CurrentPosition { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
