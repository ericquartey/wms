using System.Collections.Generic;
using Prism.Regions;

namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Fields

        private readonly IRegionNavigationJournal journal;

        private readonly PresentationMode mode;

        private readonly string notificationMessage;

        private readonly PresentationTypes presentationType;

        private readonly List<Presentation> states;

        #endregion

        #region Constructors

        public PresentationChangedMessage(string notificationMessage)
        {
            this.notificationMessage = notificationMessage;
        }

        public PresentationChangedMessage(List<Presentation> states)
        {
            this.states = states;
        }

        public PresentationChangedMessage(IRegionNavigationJournal journal)
        {
            this.journal = journal;
        }

        public PresentationChangedMessage(Presentation state)
        {
            this.states = new List<Presentation> { state };
        }

        public PresentationChangedMessage(PresentationMode mode)
        {
            this.mode = mode;
        }

        #endregion

        #region Properties

        public IRegionNavigationJournal Journal => this.journal;

        public PresentationMode Mode => this.mode;

        public string NotificationMessage => this.notificationMessage;

        public PresentationTypes PresentationType => this.presentationType;

        public List<Presentation> States => this.states;

        #endregion
    }
}
