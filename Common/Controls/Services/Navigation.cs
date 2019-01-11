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

    public class ModelChangedPubSubEvent<TModel> : Prism.Events.PubSubEvent, IPubSubEvent where TModel : IBusinessObject
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public ModelChangedPubSubEvent(object modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token { get; }

        #endregion Properties
    }

    public class ModelSelectionChangedPubSubEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IPubSubEvent where TModel : IBusinessObject
    {
        #region Fields

        private readonly int? modelId;
        private readonly string token;

        #endregion Fields

        #region Constructors

        public ModelSelectionChangedPubSubEvent(int? modelId, string token)
        {
            this.modelId = modelId;
            this.token = token;
        }

        #endregion Constructors

        #region Properties

        public int? ModelId => this.modelId;

        public string Token => this.token;

        #endregion Properties
    }

    public class RefreshModelsPubSubEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IPubSubEvent
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public RefreshModelsPubSubEvent(int modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token { get; }

        #endregion Properties
    }

    public class StatusPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Constructors

        public StatusPubSubEvent(string message = null, StatusType type = StatusType.Info)
        {
            this.Message = message?.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
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
