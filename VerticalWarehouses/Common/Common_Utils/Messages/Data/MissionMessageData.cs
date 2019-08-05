using System.Collections.ObjectModel;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
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

        #region Methods

        public override string ToString()
        {
            return $"Missions:{this.Missions?.Count ?? 0}";
        }

        #endregion
    }
}
