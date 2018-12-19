namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableView
    {
        object Data { get; set; }
        object DataContext { get; }
        string MapId { get; set; }
        string Title { get; set; }
        string Token { get; set; }
        bool IsClosed { get; set; }
        WmsViewType ViewType { get; }

        void Disappear();
    }
}
