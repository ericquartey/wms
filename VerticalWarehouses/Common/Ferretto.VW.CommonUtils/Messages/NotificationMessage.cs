using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using NLog;
using System.Diagnostics;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages
{
    public class NotificationMessage
    {

        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public NotificationMessage()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Check if we really need this constructor.")]
        public NotificationMessage(
            IMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageType type,
            BayIndex bayIndex,
            MessageStatus status = MessageStatus.NoStatus,
            ErrorLevel level = ErrorLevel.NoError,
            MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.BayIndex = bayIndex;
            this.Status = status;
            this.Verbosity = verbosity;
            this.ErrorLevel = level;

            if (Logger?.IsTraceEnabled ?? false)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(1);
                string trace = $"{sf.GetMethod().ReflectedType?.Name}.{sf.GetMethod().Name}()";

                Logger.Trace($"{source} -> {destination} - type:{type} description:\"{description}\" status:{status} [{data}][{trace}]");
            }
        }

        #endregion



        #region Properties

        public BayIndex BayIndex { get; set; }

        public IMessageData Data { get; set; }

        public string Description { get; set; }

        public MessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; set; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        public MessageType Type { get; set; }

        public MessageVerbosity Verbosity { get; } = MessageVerbosity.Info;

        #endregion
    }
}
