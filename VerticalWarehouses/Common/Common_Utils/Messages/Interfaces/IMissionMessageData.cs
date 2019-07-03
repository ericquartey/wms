using System.Collections.ObjectModel;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IMissionMessageData : IMessageData
    {
        #region Properties

        ObservableCollection<Mission> Missions { get; set; }

        #endregion
    }
}
