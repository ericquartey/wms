using System;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    // -----------------------------
    // <Idle> state (implementation)
    // The vertical axis is not calibrated
    public class NoVerticalCalibrateState : IState
    {
        #region Fields

        private StateMachineVerticalHoming _context;

        #endregion Fields

        #region Constructors

        public NoVerticalCalibrateState(StateMachineVerticalHoming parent)
        {
            this._context = parent;
        }

        #endregion Constructors

        #region Properties

        public string Type => "No Calibrate State";

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
                        this._context.ExecuteOperation(code);

                        break;
                    }
            }
        }

        #endregion Methods
    }

    public class StateMachineVerticalHoming : IState, IStateMachine
    {
        #region Fields

        //private InverterDriver driver;
        private IState _state;

        #endregion Fields

        #region Constructors

        public StateMachineVerticalHoming()
        {
            // driver = Singleton<InverterDriver>.UniqueInstance;
        }

        #endregion Constructors

        #region Properties

        public string Type => this._state.Type;

        #endregion Properties

        #region Methods

        public void ChangeState(IState newState)
        {
            this._state = newState;
        }

        public void DoAction(IdOperation code)
        {
            this._state.DoAction(code);
        }

        public void ExecuteOperation(IdOperation code)
        {
            switch (code)
            {
                case IdOperation.HorizontalHome:
                    {
                        // async inverterDriver.ExecuteAction("Horizontal Home");
                        break;
                    }
                case IdOperation.SwitchHorizontalToVertical:
                    {
                        // async inverterDriver.ExecuteAction("SwitchHorizontalToVertical");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        // async inverterDriver.ExecuteAction("Vertical Home");
                        break;
                    }
                case IdOperation.SwitchVerticalToHorizontal:
                    {
                        // async inverterDriver.ExecuteAction("SwitchVerticalToHorizontal");
                        break;
                    }
            }
        }

        public void Start()
        {
            this._state = new NoVerticalCalibrateState(this);
        }

        #endregion Methods
    }
}
