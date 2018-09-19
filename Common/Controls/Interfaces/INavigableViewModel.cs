namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigableViewModel
    {
        string MapId { get; set; }
        string Token { get; set; }
        string StateId { get; set; }
        void Appear();
        void Disappear();
    }
}
