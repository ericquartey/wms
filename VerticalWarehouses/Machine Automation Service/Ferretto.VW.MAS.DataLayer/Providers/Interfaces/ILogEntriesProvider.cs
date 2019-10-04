using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public interface ILogEntriesProvider
    {
        #region Methods

        void Add(CommandMessage command);

        void Add(NotificationMessage notification);

        #endregion
    }
}
