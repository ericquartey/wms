namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Idle
    public class HomingIdleState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public HomingIdleState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Homing state idle");

            // launch the command to switch the motor
            // ... this.driver.ExecuteHorizontalHoming();

            // this.context.ChangeState(new HorizontalSwitchDoneState(this.context, this.driver, this.data));
        }

        #endregion

        #region Properties

        public string Type => "Homing Undone State";

        #endregion

        /*
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
                        this.context.ExecuteOperation(code);
                        //TODO inverterDriver.SwitchVertToHoriz();
                        //TODO await isOperationDone();
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
        */
    }
}
