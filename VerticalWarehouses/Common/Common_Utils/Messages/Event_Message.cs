using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages
{
    public enum MessageActor
    {
        WebAPI,

        AutomationService,

        MissionScheduler,

        MissionsManager,

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
        CalibrateAxis,

        DrawerWeightDetection,

        MoveDrawerHorizontally,

        PositionDrawer,

        ProfilePosition,

        HorizontalHoming,

        AddMission,

        CreateMission,

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

        public Event_Message(IEventMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageStatus status,
            MessageType type,
            MessageVerbosity verbosity)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Status = status;
            this.Type = type;
            this.Verbosity = verbosity;
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
