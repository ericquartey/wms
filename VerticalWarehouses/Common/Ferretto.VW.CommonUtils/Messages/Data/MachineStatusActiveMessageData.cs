using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;


namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MachineStatusActiveMessageData : IMachineStatusActiveMessageData
    {
        #region Constructors

        public MachineStatusActiveMessageData()
        {
        }

        public MachineStatusActiveMessageData(MessageActor messageActor, string messageType, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MessageActor = messageActor;
            this.MessageType = messageType;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public MessageActor MessageActor { get; set; }

        public string MessageType { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"MessageActor:{this.MessageActor} MessageType:{this.MessageType} ";
        }

        #endregion
    }
}
