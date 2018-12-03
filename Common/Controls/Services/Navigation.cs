using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls.Services
{
    public enum StatusType { Info, Error, Warning, Success }

    public class ModelChangedEvent<TModel> : Prism.Events.PubSubEvent, IEventArgs where TModel : IBusinessObject
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public ModelChangedEvent(object modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token { get; }

        #endregion Properties
    }

    public class ModelSelectionChangedEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IEventArgs where TModel : IBusinessObject
    {
        #region Fields

        private readonly int? modelId;
        private readonly string token;

        #endregion Fields

        #region Constructors

        public ModelSelectionChangedEvent(int? modelId, string token)
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

    public class RefreshModelsEvent<TModel> : Prism.Events.PubSubEvent<TModel>, IEventArgs
    {
        #region Fields

        private readonly object modelId;

        #endregion Fields

        #region Constructors

        public RefreshModelsEvent(int modelId)
        {
            this.modelId = modelId;
        }

        #endregion Constructors

        #region Properties

        public object ModelId => this.modelId;

        public string Token { get; }

        #endregion Properties
    }

    public class StatusEventArgs : Prism.Events.PubSubEvent, IEventArgs
    {
        #region Constructors

        public StatusEventArgs(string message = null, StatusType type = StatusType.Info)
        {
            this.Message = message.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
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
