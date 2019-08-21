using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class PowerEnableFieldMessageData : FieldMessageData, IPowerEnableFieldMessageData
    {
        #region Constructors

        public PowerEnableFieldMessageData(bool enable, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.Enable = enable;
        }

        #endregion

        #region Properties

        public bool Enable{ get; }

        #endregion
    }
}
