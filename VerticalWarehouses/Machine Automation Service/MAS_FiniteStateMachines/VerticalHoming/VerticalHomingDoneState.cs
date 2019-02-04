using System;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineVerticalHoming context;

        #endregion Fields

        #region Constructors

        public VerticalHomingDoneState(StateMachineVerticalHoming parent)
        {
            this.context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Done State";

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
