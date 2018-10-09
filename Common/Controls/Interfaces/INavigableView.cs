namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableView
    {
        #region Properties

        object Data { get; set; }
        object DataContext { get; }
        string MapId { get; set; }
        string Title { get; set; }
        string Token { get; set; }
        WmsViewType ViewType { get; }

        #endregion Properties
    }
}
