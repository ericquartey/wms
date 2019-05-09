using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Tests
{
    public class TestView : INavigableView
    {
        #region Properties

        public WmsViewType ViewType { get; }

        public object Data { get; set; }

        public object DataContext { get; set; }

        public bool IsClosed { get; set; }

        public string MapId { get; set; }

        public string Title { get; set; }

        public string Token { get; set; }

        #endregion

        #region Methods

        public bool CanDisappear()
        {
            return true;
        }

        public void Disappear()
        {
            // TODO
        }

        #endregion
    }
}
