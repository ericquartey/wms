namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical axis homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public VerticalHomingDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Vertical homing done state");
        }

        #endregion

        #region Properties

        public string Type => "Vertical Homing Done";

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
                        //TODO inverterDriver.SwitchVertToHoriz();
                        //TODO await isOperationDone();
                        this.context.ExecuteOperation(code);
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
