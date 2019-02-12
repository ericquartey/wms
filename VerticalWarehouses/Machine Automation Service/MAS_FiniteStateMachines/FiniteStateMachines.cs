using System;
using System.Diagnostics;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : IFiniteStateMachines
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly StateMachineHoming homing;

        private readonly INewRemoteIODriver remoteIODriver;

        private readonly StateMachineVerticalHoming verticalHoming;

        #endregion

        #region Constructors

        public FiniteStateMachines(INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            var commandEvent = this.eventAggregator.GetEvent<WebAPI_CommandEvent>();
            commandEvent.Subscribe(this.DoAction);

            this.homing = new StateMachineHoming(this.driver, this.remoteIODriver, this.data, this.eventAggregator);
            this.verticalHoming = new StateMachineVerticalHoming(this.driver, this.data, this.eventAggregator);
        }

        #endregion

        #region Properties

        public StateMachineVerticalHoming StateMachineVerticalHoming => this.verticalHoming;

        #endregion

        #region Methods

        public void Destroy()
        {
            try
            {
                this.driver.Destroy();
            }
            catch (ArgumentNullException exc)
            {
                Debug.WriteLine("The inverter driver does not exist.");
                throw new ArgumentNullException("The inverter driver does not exist.", exc);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Invalid operation.");
                throw new Exception("Invalid operation", exc);
            }
        }

        public void DoAction(Command_EventParameter action)
        {
            switch (action.CommandType)
            {
                case CommandType.ExecuteHoming:
                    {
                        if (null == this.homing)
                        {
                            throw new ArgumentNullException();
                        }

                        this.homing.Start();
                        break;
                    }
                default:
                    break;
            }
        }

        public void DoHoming()
        {
            if (null == this.homing)
            {
                throw new ArgumentNullException();
            }

            this.homing.Start();
        }

        public void DoVerticalHoming()
        {
            if (null == this.verticalHoming)
            {
                throw new ArgumentNullException();
            }

            this.verticalHoming.Start();
        }

        #endregion
    }
}
