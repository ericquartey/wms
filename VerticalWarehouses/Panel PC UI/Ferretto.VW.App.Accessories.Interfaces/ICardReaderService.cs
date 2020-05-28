using System;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface ICardReaderService : IAccessoryService
    {
        #region Events

        event EventHandler<string> TokenAcquired;

        #endregion
    }
}
