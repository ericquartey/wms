using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.WMS.App.Tests
{
    public class TestView : INavigableView
    {
        #region Properties

        public object Data { get; set; }

        public object DataContext { get; set; }

        public bool IsClosed { get; set; }

        public string MapId { get; set; }

        public string Title { get; set; }

        public string Token { get; set; }

        public WmsViewType ViewType { get; }

        #endregion

        #region Methods

        public void Disappear()
        {
            // TODO
        }

        #endregion
    }
}
