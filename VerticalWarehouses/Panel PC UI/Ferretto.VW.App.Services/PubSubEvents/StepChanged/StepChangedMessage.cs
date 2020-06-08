namespace Ferretto.VW.App.Services
{
    public class StepChangedMessage
    {
        #region Constructors

        public StepChangedMessage(bool next)
        {
            this.Next = next;
            this.Back = !next;
        }

        #endregion

        #region Properties

        public bool Back { get; }

        public bool Next { get; }

        #endregion
    }
}
