namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// [EnquiryTelegramDone] event arguments.
    /// </summary>
    public class EnquiryTelegramDoneEventArgs : System.EventArgs, IEnquiryTelegramDoneEventArgs
    {
        #region Constructors

        public EnquiryTelegramDoneEventArgs(ParameterId paramID, object value, ValueDataType type)
        {
            this.ParamId = paramID;
            this.Value = value;
            this.Type = type;
        }

        #endregion

        #region Properties

        public ParameterId ParamId { get; }

        public ValueDataType Type { get; }

        public object Value { get; }

        #endregion
    }
}
