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
            this.homing = new StateMachineHoming();
            this.verticalHoming = new StateMachineVerticalHoming();
            this.driver = Singleton<MAS_InverterDriver.InverterDriver>.UniqueInstance;
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Destroy();
        }

        public void DoHoming(BroadcastDelegate broadcastDelegate)
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

        public void DoVerticalHoming(BroadcastDelegate broadcastDelegate)
        {
            if (this.verticalHoming == null)
            {
                throw new InvalidOperationException();
            }

            this.verticalHoming.Start();
            this.verticalHoming.DoAction(IdOperation.VerticalHome);
        }

        #endregion Methods
    }
}
