namespace Ferretto.VW.App.Services
{
    public class PresentationChangedMessage
    {
        #region Fields

        private readonly string errorMessage;

        private readonly PresentationMode mode;

        private readonly Presentation[] states;

        #endregion

        #region Constructors

        public PresentationChangedMessage(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        public PresentationChangedMessage(Presentation[] states)
        {
            this.states = states;
        }

        public PresentationChangedMessage(Presentation state)
        {
            this.states = new Presentation[] { state };
        }

        public PresentationChangedMessage(PresentationMode mode)
        {
            this.mode = mode;
        }

        #endregion

        #region Properties

        public string ErrorMessage => this.errorMessage;

        public PresentationMode Mode => this.mode;

        #endregion
    }
}
