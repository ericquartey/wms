namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    // Horizontal switch is done
    public class HorizontalSwitchDoneState : IState
    {
        #region Fields

        private StateMachineHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion

        #region Constructors

        public HorizontalSwitchDoneState(StateMachineHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Horizontal switch done state");
        }

        #endregion

        #region Properties

        public string Type => "Horizontal Switch Done";

        #endregion

        /*
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
        */
    }
}
