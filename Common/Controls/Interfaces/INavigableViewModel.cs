namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableViewModel : System.IDisposable
    {
        #region Properties

        object Data { get; set; }

        string MapId { get; set; }

        string StateId { get; set; }

        string Token { get; set; }

        #endregion

        #region Methods

        void Appear();

        bool CanDisappear();

        void Disappear();

        #endregion
    }
}
