namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // The vertical switch is done
    public class VerticalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public VerticalSwitchDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Vertical switch done state");
        }

        #endregion

        #region Properties

        public string Type => "Vertical Switch Done";

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
                        Console.WriteLine("Invalid operation");
                        break;
                    }
                case IdOperation.VerticalHome:
                    {
                        //TODO inverterDriver.VerticalHoming();
                        //TODO await isOperationDone();
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
        */
    }
}
