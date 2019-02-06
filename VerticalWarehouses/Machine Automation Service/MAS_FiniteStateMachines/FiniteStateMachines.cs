using System;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : IFiniteStateMachines
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly IInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHoming homing;

        private StateMachineVerticalHoming verticalHoming;

        #endregion

        #region Constructors

        public FiniteStateMachines(IInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<WebAPI_ExecuteActionEvent>().Subscribe(this.doAction);

            this.homing = new StateMachineHoming(this.driver, this.data);
            this.verticalHoming = new StateMachineVerticalHoming(this.driver, this.data, this.eventAggregator);
        }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        /// <summary>
        /// Execute complete homing.
        /// </summary>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> is thrown, if object is null.</exception>
        public void DoHoming()
        {
            if (null == this.homing)
            {
                throw new InvalidOperationException();
            }

            this.homing?.Start();
        }

        /// <summary>
        /// Execute vertical homing.
        /// </summary>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> is thrown, if object is null.</exception>
        public void DoVerticalHoming()
        {
            if (null == this.verticalHoming)
            {
                throw new InvalidOperationException();
            }

            this.verticalHoming?.Start();
        }

        /// <summary>
        /// Execute a requested action for finite state machines. Use the related state machine.
        /// </summary>
        /// <param name="actionId">A <see cref="WebAPI_Action"/> parameter related to the request</param>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> is thrown, if object is null.</exception>
        private void doAction(WebAPI_Action action)
        {
            switch (action)
            {
                case WebAPI_Action.VerticalHoming:
                    {
                        if (null == this.verticalHoming)
                        {
                            throw new InvalidOperationException();
                        }

                        this.verticalHoming?.Start();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion
    }
}
