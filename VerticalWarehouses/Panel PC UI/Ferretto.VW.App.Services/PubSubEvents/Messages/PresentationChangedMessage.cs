using System.Collections.Generic;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Constructors

        public PresentationChangedMessage(string notificationMessage)
        {
            this.NotificationMessage = notificationMessage;
        }

        public PresentationChangedMessage(System.Exception exception)
        {
            this.Exception = exception;
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

        public PresentationTypes PresentationType { get; }

        public List<Presentation> States { get; }

        #endregion
    }
}
