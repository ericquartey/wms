using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Horizontal switch is done
    public class HorizontalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        #endregion Fields

        #region Constructors

        public HorizontalSwitchDoneState(StateMachineHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Horizontal Switch Done";

        #endregion Properties

        #region Methods

        public void DoAction(IdOperation code)
        {
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        //TODO await inverterDriver.HorizontalHoming();
                        //TODO await isOperationDone();
                        this.context.ExecuteOperation(code);
                        //TODO if (_context.HorizontalHomingAlreadyDone && operationSuccess) { this._context.ChangeState(new HomingDoneState(this._context)); }
                        //TODO else
                        this.context.ChangeState(new HorizontalHomingDoneState(this.context));
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
