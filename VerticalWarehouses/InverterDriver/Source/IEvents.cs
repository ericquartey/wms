namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    /// IConnectedEvent: interface definition for the event [Connected]
    /// </summary>
    public interface IConnectedEvent
    {
        #region Methods

        void Connected(object sender, ConnectedEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// IConnectedEventArgs: interface definition for [Connected] event arguments.
    /// </summary>
    public interface IConnectedEventArgs
    {
        #region Properties

        bool State { get; }

        #endregion Properties
    }

    /// <summary>
    /// IEnquiryTelegramDone: interface definition for the event [EnquiryTelegramDone]
    /// </summary>
    public interface IEnquiryTelegramDone
    {
        #region Methods

        void EnquiryTelegramDone(object sender, EnquiryTelegramDoneEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// IEnquiryTelegramDoneEventArgs: interface definition for [EnquiryTelegramDone] event arguments.
    /// </summary>
    public interface IEnquiryTelegramDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Get the parameter ID code.
        /// </summary>
        ParameterID ParamID { get; }

        /// <summary>
        /// Get the parameter value.
        /// </summary>
        object Value { get; }

        #endregion Properties
    }

    /// <summary>
    /// IErrorEvent: interface definition for the event [Error]
    /// </summary>
    public interface IErrorEvent
    {
        #region Methods

        void Error(object sender, ErrorEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// IErrorEventArgs: interface definition for [Error] event arguments.
    /// </summary>
    /// TODO: To be removed.
    public interface IErrorEventArgs
    {
        #region Properties

        /// <summary>
        /// Error code.
        /// </summary>
        InverterDriverErrors ErrorCode { get; }

        #endregion Properties
    }

    /// <summary>
    /// ILastRequestDone: interface definition for the event [LastRequestDone].
    /// </summary>
    public interface ILastRequestDone
    {
        #region Methods

        void LastRequestDone(object sender, LastRequestDoneEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// ILastRequestDoneEventArgs: interface definition for [LastRequestDone] event arguments.
    /// </summary>
    public interface ILastRequestDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// <c>True </c> if last request has been done.
        /// </summary>
        bool State { get; }

        #endregion Properties
    }

    /// <summary>
    /// ISelectTelegramDone: interface definition for the event [SelectTelegramDone]
    /// </summary>
    public interface ISelectTelegramDone
    {
        #region Methods

        void SelectTelegramDone(object sender, SelectTelegramDoneEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// ISelectTelegramDoneEventArgs: interface definition for [SelectTelegramDone] event arguments.
    /// </summary>
    public interface ISelectTelegramDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Get the parameter ID code.
        /// </summary>
        ParameterID ParamID { get; }

        /// <summary>
        /// Get the parameter value.
        /// </summary>
        object Value { get; }

        #endregion Properties
    }
}
