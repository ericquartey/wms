using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is undone
    public class VerticalHomingUndoneState : IState
    {
        #region Fields

        private StateMachineVerticalHoming context;

        #endregion Fields

        #region Constructors

        public VerticalHomingUndoneState(StateMachineVerticalHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Undone State";

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
                        // inverterDriver.VerticalHome();
                        // await isOperationDone();
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
