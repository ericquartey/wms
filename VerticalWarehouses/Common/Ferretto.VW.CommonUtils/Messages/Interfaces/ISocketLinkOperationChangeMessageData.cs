using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISocketLinkOperationChangeMessageData : IMessageData
    {
        #region Properties

        BayNumber BayNumber { get; set; }

        int? CompartmentX1Position { get; set; }

        int? CompartmentX2Position { get; set; }

        int? CompartmentY1Position { get; set; }

        int? CompartmentY2Position { get; set; }

        string Id { get; set; }

        string ItemCode { get; set; }

        string ItemDescription { get; set; }

        string ItemListCode { get; set; }

        string Message { get; set; }

        string OperationType { get; set; }

        double RequestedQuantity { get; set; }

        #endregion
    }
}
