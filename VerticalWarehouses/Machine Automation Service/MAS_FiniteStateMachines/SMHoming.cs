using System;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    // Define an enumerative for all available operation performed by the inverter
    public enum IdOperation
    {
        HorizontalHome = 0,

        VerticalHome = 1,

        SwitchHorizontalToVertical = 2,

        SwitchVerticalToHorizontal = 3
    }

    // -----------------------------------------------
    // <Horizontal homing done> state (implementation)
    // The horizontal axis homing is done
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming _context;

        #endregion Fields

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent)
        {
            this._context = parent;
            this._context.HorizontalHomingAlreadyDone = true;
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
                        // inverterDriver.SwitchHorizToVert();
                        // await isOperationDone();
                        this._context.ExecuteOperation(code);
                        this._context.ChangeState(new VerticalSwitchDoneState(this._context));
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
            }
        }

        #endregion Methods
    }

    // -----------------------------------------------
    // <Horizontal switch done> state (implementation)
    // The horizontal axis is selected and the related switch is made successfully
    public class HorizontalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming _context;

        #endregion Fields

        #region Constructors

        public HorizontalSwitchDoneState(StateMachineHoming parent)
        {
            this._context = parent;
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
                        // inverterDriver.HorizontalHoming();
                        // await isOperationDone();
                        this._context.ExecuteOperation(code);
                        //if (_context.HorizontalHomingAlreadyDone && operationSuccess) { return; }

                        this._context.ChangeState(new HorizontalHomingDoneState(this._context));
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
            }
        }

        #endregion Methods
    }

    // -----------------------------
    // <Idle> state (implementation)
    // The vertical and horizontal axis are not calibrated
    public class NoCalibrateState : IState
    {
        #region Fields

        private StateMachineHoming _context;

        #endregion Fields

        #region Constructors

        public NoCalibrateState(StateMachineHoming parent)
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
                        this._context.ExecuteOperation(code);
                        // inverterDriver.SwitchVertToHoriz();
                        // await isOperationDone();
                        this._context.ChangeState(new HorizontalSwitchDoneState(this._context));
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
            }
        }

        #endregion Methods
    }

    // ----------------------------------
    // State Machine for complete homing
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        //private InverterDriver driver;
        private IState _state;

        #endregion Fields

        #region Constructors

        public StateMachineHoming()
        {
            // driver = Singleton<InverterDriver>.UniqueInstance;
        }

        #endregion Constructors

        #region Properties

        public bool HorizontalHomingAlreadyDone { get; set; }

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
            this.HorizontalHomingAlreadyDone = false;
            // check the sensors before to set the initial state, if error exit ->
            this._state = new NoCalibrateState(this);
        }

        #endregion Methods
    }

    // ---------------------------------------------
    // <Vertical homing done> state (implementation)
    // The vertical axis homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming _context;

        #endregion Fields

        #region Constructors

        public VerticalHomingDoneState(StateMachineHoming parent)
        {
            this._context = parent;
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
                        // inverterDriver.SwitchVertToHoriz();
                        // await isOperationDone();
                        this._context.ExecuteOperation(code);
                        this._context.ChangeState(new HorizontalSwitchDoneState(this._context));
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        Console.WriteLine("Invalid operation");
                        break;
                    }
            }
        }

        #endregion Methods
    }

    // ---------------------------------------------
    // <Vertical switch done> state (implementation)
    // The vertical axis is selected and the related switch is made successfully
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming _context;

        #endregion Fields

        #region Constructors

        public VerticalSwitchDoneState(StateMachineHoming parent)
        {
            this._context = parent;
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
                        // inverterDriver.VerticalHoming();
                        // await isOperationDone();
                        this._context.ExecuteOperation(code);
                        this._context.ChangeState(new VerticalHomingDoneState(this._context));
                        break;
                    }
            }
        }

        #endregion Methods
    }
}
