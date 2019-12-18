using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines
{
    internal abstract class LaserStateMachineBase : ILaserStateMachine
    {
        #region Fields

        private readonly object lockObj = new object();

        #endregion

        #region Constructors

        public LaserStateMachineBase(
            IEventAggregator eventAggregator,
            ILogger logger,
            BlockingConcurrentQueue<FieldCommandMessage> laserCommandQueue)
        {
            this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.LaserCommandQueue = laserCommandQueue ?? throw new ArgumentNullException(nameof(laserCommandQueue));
        }

        #endregion

        #region Properties

        protected ILaserState CurrentState { get; private set; }

        protected IEventAggregator EventAggregator { get; }

        protected BlockingConcurrentQueue<FieldCommandMessage> LaserCommandQueue { get; }

        protected ILogger Logger { get; }

        #endregion

        #region Methods

        public void ChangeState(ILaserState newState)
        {
            if (newState is null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            lock (this.lockObj)
            {
                if (this.CurrentState is IDisposable disposableState)
                {
                    disposableState.Dispose();
                }

                this.CurrentState = newState;
                newState.Start();
            }
        }

        public void EnqueueMessage(FieldCommandMessage message)
        {
            this.LaserCommandQueue.Enqueue(message);
        }

        public void ProcessResponseMessage(string message)
        {
            lock (this.lockObj)
            {
                this.CurrentState?.ProcessResponseMessage(message);
            }
        }

        public abstract void Start();

        #endregion
    }
}
