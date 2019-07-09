namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// Interface definition for [SelectTelegramDone] event arguments.
    /// </summary>
    public interface ISelectTelegramDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the parameter Id.
        /// </summary>
        ParameterId ParamId { get; }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        object Value { get; }

        #endregion
    }
}
