using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class ResetSecurityFieldMessageData : FieldMessageData, IResetSecurityFieldMessageData
    {
        #region Constructors

        public ResetSecurityFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion
    }
}
