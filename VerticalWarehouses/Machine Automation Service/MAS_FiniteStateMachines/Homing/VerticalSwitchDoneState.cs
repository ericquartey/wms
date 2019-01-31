using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical switch is done
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        #endregion Fields

        #region Constructors

        public VerticalSwitchDoneState(StateMachineHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Switch Done";

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
                        Console.WriteLine("Invalid operation");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        //TODO inverterDriver.VerticalHoming();
                        //TODO await isOperationDone();
                        this.context.ExecuteOperation(code);
                        this.context.ChangeState(new VerticalHomingDoneState(this.context));
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
