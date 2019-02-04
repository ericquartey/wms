using System;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class FiniteStateMachines : IFiniteStateMachines
    {
        #region Fields

        private MAS_InverterDriver.InverterDriver driver;

        private StateMachineHoming homing;

        private StateMachineVerticalHoming verticalHoming;

        #endregion Fields

        #region Constructors

        public FiniteStateMachines()
        {
            this.driver = Singleton<MAS_InverterDriver.InverterDriver>.UniqueInstance;
            this.homing = new StateMachineHoming(this);
            this.verticalHoming = new StateMachineVerticalHoming(this);
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        public void DoHoming()
        {
            if (this.homing == null)
            {
                throw new InvalidOperationException();
            }

            this.homing.Start();
            this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            this.homing.DoAction(IdOperation.HorizontalHome);
            this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
            this.homing.DoAction(IdOperation.VerticalHome);
            this.homing.DoAction(IdOperation.SwitchVerticalToHorizontal);
            this.homing.DoAction(IdOperation.HorizontalHome);
            this.homing.DoAction(IdOperation.SwitchHorizontalToVertical);
        }

        public void DoVerticalHoming()
        {
            if (this.verticalHoming == null)
            {
                throw new InvalidOperationException();
            }

            this.verticalHoming.Start();
            this.verticalHoming.DoAction(IdOperation.VerticalHome);
        }

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

        #endregion Methods
    }
}
