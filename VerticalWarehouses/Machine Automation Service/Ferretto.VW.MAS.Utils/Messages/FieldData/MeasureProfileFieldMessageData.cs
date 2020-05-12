using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class MeasureProfileFieldMessageData : FieldMessageData, IMeasureProfileFieldMessageData
    {
        #region Constructors

        public MeasureProfileFieldMessageData(bool enable = false, ushort profile = 0, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.Enable = enable;
            this.Profile = profile;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        public ushort Profile { get; }

        #endregion
    }
}
