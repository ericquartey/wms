using System;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
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

        #endregion Fields

        #region Constructors

        public FiniteStateMachines(IInverterDriver iDriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            var commandEvent = this.eventAggregator.GetEvent<WebAPI_CommandEvent>();
            commandEvent.Subscribe(this.DoAction);

            this.homing = new StateMachineHoming(this.driver, this.data);
            this.verticalHoming = new StateMachineVerticalHoming(this.driver, this.data, this.eventAggregator);
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        public void DoHoming()
        {
            if (null == this.homing)
            {
                throw new ArgumentNullException();
            }

            this.homing?.Start();
        }

        public void DoVerticalHoming()
        {
            if (null == this.verticalHoming)
            {
                throw new ArgumentNullException();
            }

            this.verticalHoming?.Start();
        }

        public void DoAction(Command_EventParameter action)
        {
            switch (action.CommandType)
            {
                case CommandType.ExecuteHoming:
                    {
                        if (null == this.verticalHoming)
                        {
                            throw new ArgumentNullException();
                        }

                        this.verticalHoming.Start();
                        break;
                    }
                default:
                    break;
            }
        }

        #endregion Methods
    }
}
