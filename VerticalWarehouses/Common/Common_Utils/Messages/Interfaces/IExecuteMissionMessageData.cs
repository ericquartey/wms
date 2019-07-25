using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IExecuteMissionMessageData : IMessageData
    {
        #region Properties

        string BayConnectionId { get; set; }

        Mission Mission { get; set; }

        int MissionsQuantity { get; set; }

        #endregion
    }
}
