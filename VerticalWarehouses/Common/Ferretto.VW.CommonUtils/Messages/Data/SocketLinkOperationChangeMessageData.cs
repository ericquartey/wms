using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SocketLinkOperationChangeMessageData : ISocketLinkOperationChangeMessageData
    {
        #region Constructors

        public SocketLinkOperationChangeMessageData()
        {
        }

        public SocketLinkOperationChangeMessageData(
            BayNumber bayNumber,
            string id,
            string message,
            double? quantity,
            string operationType,
            string itemCode,
            string itemDescription,
            string itemListCode,
            int? x1 = null,
            int? x2 = null,
            int? y1 = null,
            int? y2 = null,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.BayNumber = bayNumber;
            this.Id = id;
            this.Message = message;
            if (quantity.HasValue)
            {
                this.RequestedQuantity = quantity.Value;
            }
            this.OperationType = operationType;
            this.ItemCode = itemCode;
            this.ItemListCode = itemListCode;
            this.ItemDescription = itemDescription;
            this.CompartmentX1Position = x1;
            this.CompartmentX2Position = x2;
            this.CompartmentY1Position = y1;
            this.CompartmentY2Position = y2;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        public int? CompartmentX1Position { get; set; }

        public int? CompartmentX2Position { get; set; }

        public int? CompartmentY1Position { get; set; }

        public int? CompartmentY2Position { get; set; }

        public string Id { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public string ItemListCode { get; set; }

        public string Message { get; set; }

        public string OperationType { get; set; }

        public double RequestedQuantity { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
