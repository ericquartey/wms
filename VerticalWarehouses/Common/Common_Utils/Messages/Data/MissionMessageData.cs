using System.Collections.ObjectModel;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionMessageData : IMissionMessageData
    {
        #region Constructors

        public MissionMessageData(ObservableCollection<Mission> missions, MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Missions = missions;
        }

        #endregion

        #region Properties

        public ObservableCollection<Mission> Missions { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
