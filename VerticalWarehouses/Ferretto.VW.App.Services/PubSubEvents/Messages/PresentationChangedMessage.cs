using System.Collections.Generic;

namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Fields

        private readonly string errorMessage;

        private readonly PresentationMode mode;

        private readonly PresentationTypes presentationType;

        private readonly List<Presentation> states;

        #endregion

        #region Constructors

        public PresentationChangedMessage(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        public PresentationChangedMessage(List<Presentation> states)
        {
            this.states = states;
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

        public string ErrorMessage => this.errorMessage;

        public PresentationMode Mode => this.mode;

        public PresentationTypes PresentationType => this.presentationType;

        public List<Presentation> States => this.states;

        #endregion
    }
}
