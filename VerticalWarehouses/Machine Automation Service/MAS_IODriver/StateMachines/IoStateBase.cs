using System;
using Ferretto.VW.MAS.IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines
{
    public abstract class IoStateBase : IIoState
    {

        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public IoStateBase(IIoStateMachine parentStateMachine, ILogger logger)
        {
            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger;
        }

        #endregion

        #region Destructors

        ~IoStateBase()
        {
            this.Dispose(false);
        }

        #endregion



        #region Properties

        protected ILogger Logger { get; }

        protected IIoStateMachine ParentStateMachine { get; }

        public virtual string Type => this.GetType().ToString();

        #endregion



        #region Methods

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void ProcessMessage(IoMessage message);

        public abstract void ProcessResponseMessage(IoReadMessage message);

        public abstract void Start();

        #endregion
    }
}
