using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The horizontal axis homing is done
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        #endregion Fields

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent)
        {
            this.context = parent;
            this.context.HorizontalHomingAlreadyDone = true;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Horizontal homing done";

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
                        //TODO inverterDriver.SwitchHorizToVert();
                        //TODO await isOperationDone();
                        this.context.ExecuteOperation(code);
                        this.context.ChangeState(new VerticalSwitchDoneState(this.context));
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        Console.WriteLine("Invalid operation");
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
