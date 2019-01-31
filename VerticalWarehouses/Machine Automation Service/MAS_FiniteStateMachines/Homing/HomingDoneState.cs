using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Complete homing is done
    public class HomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        #endregion Fields

        #region Constructors

        public HomingDoneState(StateMachineHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Homing Done State";

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
