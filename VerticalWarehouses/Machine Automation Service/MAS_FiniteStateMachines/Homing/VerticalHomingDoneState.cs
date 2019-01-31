using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical axis homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        #endregion Fields

        #region Constructors

        public VerticalHomingDoneState(StateMachineHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Done";

        #endregion Properties

        #region Methods

        public void DoAction(IdOperation code)
        {
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
                case IdOperation.SwitchHorizontalToVertical:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        //TODO inverterDriver.SwitchVertToHoriz();
                        //TODO await isOperationDone();
                        this.context.ExecuteOperation(code);
                        this.context.ChangeState(new HorizontalSwitchDoneState(this.context));
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
            }
        }

        #endregion Methods
    }
}
