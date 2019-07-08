namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// IEnquiryTelegramDoneEventArgs: interface definition for [EnquiryTelegramDone] event arguments.
    /// </summary>
    public interface IEnquiryTelegramDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the parameter ID code.
        /// </summary>
        ParameterId ParamId { get; }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        object Value { get; }

        #endregion
    }
}
