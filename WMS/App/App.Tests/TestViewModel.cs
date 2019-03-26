using Ferretto.Common.Controls.Interfaces;

namespace Ferretto.WMS.App.Tests
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
#pragma warning disable CA1063 // Implement IDisposable Correctly

    public class TestViewModel : INavigableViewModel
#pragma warning restore CA1063 // Implement IDisposable Correctly
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
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

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA1063 // Implement IDisposable Correctly

        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            // Test method. Nothing to do here.
        }

        #endregion
    }
}
