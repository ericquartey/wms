namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is undone
    public class VerticalHomingIdleState : IState
    {
        #region Fields

        private StateMachineVerticalHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion Fields

        #region Constructors

        public VerticalHomingIdleState(StateMachineVerticalHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Start vertical homing");

            // launch the command
            this.driver.ExecuteVerticalHoming();

            this.context.ChangeState(new VerticalHomingDoneState(this.context, this.driver, this.data));
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Idle State";

        #endregion Properties

        #region Methods

        public void DoAction(IdOperation code)
        {
            /*
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
            */
        }

        #endregion Methods
    }
}
