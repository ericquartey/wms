using System;
using System.Threading.Tasks;
using Ferretto.VW.Devices.TokenReader;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface ITokenReaderService : IAccessoryService
    {
        bool IsTokenInserted { get; }
        string TokenSerialNumber { get; }
        #region Events

        event EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion

        #region Methods

        Task UpdateSettingsAsync(bool isEnabled, string portName);

        #endregion
    }
}
