using System.Collections.Generic;
using Ferretto.VW.App.Services.Models;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Constructors

        public PresentationChangedMessage(string notificationMessage, NotificationSeverity notificationSeverity)
        {
            this.NotificationMessage = notificationMessage == string.Empty ? null : notificationMessage;
            this.NotificationSeverity = notificationSeverity;
        }

        public PresentationChangedMessage(System.Exception exception)
        {
            this.Exception = exception;
            this.NotificationSeverity = NotificationSeverity.Error;
        }

        public PresentationChangedMessage(List<Presentation> states)
        {
            this.States = states;
        }

        public PresentationChangedMessage(IRegionNavigationJournal journal)
        {
            this.Journal = journal;
        }

        public PresentationChangedMessage(Presentation state)
        {
            this.States = new List<Presentation> { state };
        }

        public PresentationChangedMessage(PresentationMode mode)
        {
            this.Mode = mode;
        }

        #endregion

        #region Properties

        public System.Exception Exception { get; }

        public IRegionNavigationJournal Journal { get; }

        public PresentationMode Mode { get; }

        public string NotificationMessage { get; }

        public NotificationSeverity NotificationSeverity { get; }

        public PresentationTypes PresentationType { get; }

        public List<Presentation> States { get; }

        #endregion
    }
}
