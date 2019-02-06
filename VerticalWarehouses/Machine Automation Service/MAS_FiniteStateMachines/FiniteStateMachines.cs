using System;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : IFiniteStateMachines
    {
        #region Fields

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        private StateMachineHoming homing;

        private StateMachineVerticalHoming verticalHoming;

        #endregion

        #region Constructors

        public FiniteStateMachines(MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.homing = new StateMachineHoming(this.driver, this.data);
            this.verticalHoming = new StateMachineVerticalHoming(this.driver, this.data);
        }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        /// <summary>
        /// Execute complete homing.
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> is thrown if object is null.</exception>
        /// </summary>
        public void DoHoming()
        {
            if (this.homing == null)
            {
                throw new InvalidOperationException();
            }

            this.homing.Start();
        }

        /// <summary>
        /// Execute vertical homing.
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> is thrown if object is null.</exception>
        /// </summary>
        public void DoVerticalHoming()
        {
            if (this.verticalHoming == null)
            {
                throw new InvalidOperationException();
            }

            this.data.LogWriting("Do vertical homing");

            this.verticalHoming.Start();

            this.data.LogWriting("End homing");
        }

        #endregion
    }
}
