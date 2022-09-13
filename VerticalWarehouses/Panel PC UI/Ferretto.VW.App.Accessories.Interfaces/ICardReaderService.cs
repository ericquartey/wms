using System;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface ICardReaderService : IAccessoryService
    {
        #region Events

        event EventHandler<string> KeysAcquired;

        event EventHandler<RegexMatchEventArgs> TokenAcquired;

        #endregion

        #region Methods

        Task UpdateSettingsAsync(bool isEnabled, string tokenRegex, bool isLocal);

        #endregion
    }
}
