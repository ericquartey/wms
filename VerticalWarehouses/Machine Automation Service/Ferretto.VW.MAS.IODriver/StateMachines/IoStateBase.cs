using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines
{
    internal abstract class IoStateBase : IIoState
    {
        #region Constructors

        public IoStateBase(IIoStateMachine parentStateMachine, ILogger logger)
        {
            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger;
        }

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        protected ILogger Logger { get; }

        protected IIoStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

        public abstract void ProcessResponseMessage(IoReadMessage message);

        public abstract void Start();

        #endregion
    }
}
