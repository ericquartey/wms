using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class LogoutMessageData : ILogoutMessageData
    {
        #region Constructors

        public LogoutMessageData()
        {
        }

        public LogoutMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Logout command Verbosity:{this.Verbosity}";
        }

        #endregion

    }
}
