using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Devices.TokenReader;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface ITokenReaderService : IAccessoryService
    {
        #region Fields

        event EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion
    }
}
