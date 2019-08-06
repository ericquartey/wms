using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class PowerEnableFieldMessageData : IPowerEnableFieldMessageData
    {
        #region Constructors

        public PowerEnableFieldMessageData(bool enable, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public bool Enable{ get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
