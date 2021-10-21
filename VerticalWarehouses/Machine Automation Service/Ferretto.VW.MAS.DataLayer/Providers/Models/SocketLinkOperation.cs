using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public class SocketLinkOperation
    {
        #region Constructors

        public SocketLinkOperation()
        {
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        public int? CompartmentX1Position { get; set; }

        public int? CompartmentX2Position { get; set; }

        public int? CompartmentY1Position { get; set; }

        public int? CompartmentY2Position { get; set; }

        public DateTimeOffset? CompletedTime { get; set; }

        public double? ConfirmedQuantity { get; set; }

        public string Id { get; set; }

        public bool? IsCompleted { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public string ItemListCode { get; set; }

        public string Message { get; set; }

        public string OperationType { get; set; }

        public double? RequestedQuantity { get; set; }

        #endregion
    }
}
