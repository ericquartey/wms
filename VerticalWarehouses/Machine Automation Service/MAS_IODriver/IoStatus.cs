namespace Ferretto.VW.MAS_IODriver
{
    public class IoStatus
    {
        #region Fields

        private const int totalInputs = 8;

        private const int totalOutputs = 5;

        private bool[] inputs;

        private bool[] outputs;

        #endregion

        #region Constructors

        public IoStatus()
        {
            this.inputs = new bool[totalInputs];
            this.outputs = new bool[totalOutputs];
        }

        public IoStatus(IoStatus status)
        {
            this.inputs = new bool[totalInputs];
            this.outputs = new bool[totalOutputs];
        }

        #endregion

        #region Methods

        public bool UpdateInputStates(bool[] newInputStates)
        {
            return false;
        }

        #endregion
    }
}
