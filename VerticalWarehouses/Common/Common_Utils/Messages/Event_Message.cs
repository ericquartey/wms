using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages
{
    public enum MessageActor
    {
        WebAPI,

        AutomationService,

        MissionScheduler,

        MachineManager,

        FiniteStateMachines,

        InverterDriver,

        IODriver,

        DataLayer,

        Any
    }

    public enum MessageStatus
    {
        Start,

        Error,

        End
    }

    public enum MessageType
    {
        HorizontalHoming,

        AddMission,

        StartAction,

        StopAction,

        EndAction,

        ErrorAction
    }

    public enum MessageVerbosity
    {
        Debug,

        Info
    }

    public class Event_Message
    {
        #region Constructors

        public Event_Message()
        {
        }

        public Event_Message( IEventMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageStatus status,
            MessageType type,
            MessageVerbosity verbosity )
        {
            Data = data;
            Description = description;
            Destination = destination;
            Source = source;
            Status = status;
            Type = type;
            Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public IEventMessageData Data { get; private set; }

        public string Description { get; private set; }

        public MessageActor Destination { get; set; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
