using System.Collections.Generic;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Constructors

        public PresentationChangedMessage(List<IPresentation> states)
        {
            this.States = states;
        }

        public PresentationChangedMessage(Presentation state)
        {
            this.States = new List<IPresentation> { state };
        }

        public PresentationChangedMessage(PresentationStep stateStep)
        {
            this.States = new List<IPresentation> { stateStep };
        }

        public PresentationChangedMessage(PresentationMode mode)
        {
            this.Mode = mode;
        }

        #endregion

        #region Properties

        public PresentationMode Mode { get; }

        public NotificationSeverity NotificationSeverity { get; }

        public PresentationTypes PresentationType { get; }

        public List<IPresentation> States { get; }

        #endregion
    }
}
