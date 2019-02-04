namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    // Vertical homing is done
    public class VerticalHomingDoneState : IState
    {
        #region Fields

        private StateMachineVerticalHoming context;

        private MAS_DataLayer.IWriteLogService data;

        private MAS_InverterDriver.IInverterDriver driver;

        #endregion Fields

        #region Constructors

        public VerticalHomingDoneState(StateMachineVerticalHoming parent, MAS_InverterDriver.IInverterDriver iDriver, MAS_DataLayer.IWriteLogService iWriteLogService)
        {
            this.context = parent;
            this.driver = iDriver;
            this.data = iWriteLogService;

            this.data.LogWriting("Done vertical homing");
        }

        #endregion Constructors

        #region Properties

        public string Type => "Vertical Homing Done State";

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
                        Console.WriteLine("Invalid operation");
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
