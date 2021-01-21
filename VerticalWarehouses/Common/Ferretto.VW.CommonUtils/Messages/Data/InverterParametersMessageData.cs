using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterParametersMessageData : IInverterParametersMessageData
    {
        #region Constructors

        public InverterParametersMessageData()
        {
        }

        public InverterParametersMessageData(
            short code,
            int dataset,
            string value,
            bool isReadMessage,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Code = code;
            this.Datset = dataset;
            this.Value = value;
            this.IsReadMessage = isReadMessage;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public short Code { get; set; }

        public int Datset { get; set; }

        public bool IsReadMessage { get; set; }

        public string Value { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Code={this.Code}, Dataset={this.Datset}, Value={this.Value}, IsReadMessage={this.IsReadMessage}";
        }

        #endregion
    }
}
