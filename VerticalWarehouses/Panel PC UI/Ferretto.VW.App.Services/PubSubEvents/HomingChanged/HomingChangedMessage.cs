namespace Ferretto.VW.App.Services
{
    public class HomingChangedMessage
    {
        #region Constructors

        public HomingChangedMessage(bool isHoming)
        {
            this.IsHoming = isHoming;
        }

        #endregion

        #region Properties

        public bool IsHoming { get; }

        #endregion
    }
}
