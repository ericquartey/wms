using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IStatisticsDataProvider
    {
        #region Methods

        int MissionTotalNumber();

        double TotalDistance();

        #endregion
    }
}
