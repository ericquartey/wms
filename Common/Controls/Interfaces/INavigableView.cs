namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableView
    {
        #region Properties

        object DataContext { get; }
        string MapId { get; set; }
        string Title { get; set; }
        string Token { get; set; }

        #endregion Properties
    }
}
