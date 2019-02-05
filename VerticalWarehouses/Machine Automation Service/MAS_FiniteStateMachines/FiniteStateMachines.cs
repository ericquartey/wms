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

            //this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            //this.homing.DoAction(IdOperation.HorizontalHome);
            //this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
            //this.homing.DoAction(IdOperation.VerticalHome);
            //this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            //this.homing.DoAction(IdOperation.HorizontalHome);
            //this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
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
            //this.verticalHoming.DoAction(IdOperation.VerticalHome);

            this.data.LogWriting("End homing");
        }

        #endregion

        /*
        public void MakeOperationByInverter(IdOperation code)
        {
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        // TODO await driver.ExecuteAction("Horizontal Home");
                        break;
                    }
                case IdOperation.SwitchHorizontalToVertical:
                    {
                        // TODO await driver.ExecuteAction("SwitchHorizontalToVertical");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        this.driver.ExecuteVerticalHoming();
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        // TODO await driver.ExecuteAction("SwitchVerticalToHorizontal");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        */
    }
}
