using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public enum StatusType
    {
        None,

        Info,

        Error,

        Warning,

        Success
    }

    public class StatusPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Constructors

        public StatusPubSubEvent(string message = null, StatusType type = StatusType.Info)
        {
            this.Message = message?.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            this.Type = type;
        }

        #endregion Constructors

        #region Properties

        public bool IsSchedulerOnline { get; set; }

        public string Message { get; set; }

        public string Token { get; }

        public StatusType Type { get; set; }

        #endregion Properties
    }
}
