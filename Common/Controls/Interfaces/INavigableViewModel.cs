namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableViewModel
    {
        #region Properties

        object Data { get; set; }
        string MapId { get; set; }
        string StateId { get; set; }
        string Token { get; set; }

        #endregion Properties

        #region Methods

        void Appear();

        void Disappear();

        void Dispose();

        #endregion Methods
    }
}
