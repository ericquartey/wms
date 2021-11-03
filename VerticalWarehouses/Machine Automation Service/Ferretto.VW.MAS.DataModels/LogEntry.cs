using System;
using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LogEntry : DataModel
    {
        #region Properties

        public BayNumber BayNumber { get; set; }

        public string Data { get; set; }

        public string Description { get; set; }

        public MessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; set; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        [NotMapped]
        public BayNumber TargetBay { get; set; }

        public DateTime TimeStamp { get; set; }

        public MessageType Type { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.BayNumber,-15}|{this.TargetBay,-15}|{this.Source,-20}|{this.Destination,-20}|{this.Type,-20}|{this.Status,-20}|{this.ErrorLevel,-20}|{this.Description,-100}|{this.Data}";
        }

        #endregion
    }
}
