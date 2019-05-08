namespace Ferretto.WMS.App.Controls.Interfaces
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

        bool CanDisappear();

        void Disappear();
    }
}
