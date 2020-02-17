using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;


namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MachineStateActiveMessageData : IMachineStateActiveMessageData
    {
        #region Constructors

        public MachineStateActiveMessageData()
        {
        }

        public MachineStateActiveMessageData(MessageActor messageActor, string currentState, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MessageActor = messageActor;
            this.CurrentState = currentState;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public string CurrentState { get; set; }

        public MessageActor MessageActor { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"MessageActor:{this.MessageActor} CurrentState:{this.CurrentState} ";
        }

        #endregion
    }
}
