using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.WMS.App.Tests
{
    public class TestViewModel : INavigableViewModel
    {
        #region Properties

        public object Data { get; set; }

        public string MapId { get; set; }

        public string StateId { get; set; }

        public string Token { get; set; }

        #endregion

        #region Methods

        public void Appear()
        {
            // Test method. Nothing to do here.
        }

        public bool CanDisappear()
        {
            return true;
        }

        public void Disappear()
        {
            // Test method. Nothing to do here.
        }

        public void Dispose()
        {
            // Test method. Implement dispose
        }

        #endregion
    }
}
