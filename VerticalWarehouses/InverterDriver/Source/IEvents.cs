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
    /// IGetMessageFromServerEvent: interface definition for the event [GetMessageFromServer]
    /// </summary>
    /// NOTE: To be removed.
    public interface IGetMessageFromServerEvent
    {
        #region Methods

        /// <summary>
        /// GetMessageFromServer Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void GetMessageFromServer(object sender, GetMessageFromServerEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// IGetMessageFromServerEventArgs: interface definition for [GetMessageFromServer] event arguments.
    /// </summary>
    /// TODO: To be removed.
    public interface IGetMessageFromServerEventArgs
    {
        #region Properties

        /// <summary>
        /// Command Id.
        /// </summary>
        CommandId CmdId { get; }

        /// <summary>
        /// Message.
        /// </summary>
        string Message { get; }

        #endregion Properties
    }

    /// <summary>
    /// IOperationDoneEvent: interface definition for the event [OperationDone]
    /// </summary>
    public interface IOperationDoneEvent
    {
        #region Methods

        void OperationDone(object sender, OperationDoneEventArgs eventArgs);

        #endregion Methods
    }

    /// <summary>
    /// IOperationDoneEventArgs: interface definition for [OperationDone] event arguments.
    /// </summary>
    /// TODO: To be removed.
    public interface IOperationDoneEventArgs
    {
        #region Properties

        /// <summary>
        /// Command Id.
        /// </summary>
        CommandId CmdId { get; }

        /// <summary>
        /// Operation result.
        /// </summary>
        bool Result { get; }

        #endregion Properties
    }
}
