using System;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    ///Delegate for the [Connected] event.
    /// </summary>
    public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [Error] event.
    /// </summary>
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs eventArgs);

    /// <summary>
    /// Delegate for Getting Messages From the Server.
    /// </summary>
    /// NOTE: To be removed
    public delegate void GetMessageFromServerEventHandler(object sender, GetMessageFromServerEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [OperationDone] event.
    /// </summary>
    public delegate void OperationDoneEventHandler(object sender, OperationDoneEventArgs eventArgs);

    /// <summary>
    /// [Connected] event arguments.
    /// </summary>
    public class ConnectedEventArgs : EventArgs, IConnectedEventArgs
    {
        #region Fields

        private readonly bool state;

        #endregion Fields

        #region Constructors

        public ConnectedEventArgs(bool State)
        {
            this.state = State;
        }

        #endregion Constructors

        #region Properties

        public bool State => this.state;

        #endregion Properties
    }

    /// <summary>
    /// [Error] event arguments.
    /// </summary>
    public class ErrorEventArgs : EventArgs, IErrorEventArgs
    {
        #region Fields

        private readonly InverterDriverErrors errorCode;

        #endregion Fields

        #region Constructors

        public ErrorEventArgs(InverterDriverErrors error)
        {
            this.errorCode = error;
        }

        #endregion Constructors

        #region Properties

        public InverterDriverErrors ErrorCode => this.errorCode;

        #endregion Properties
    }

    /// <summary>
    /// [GetMessageFromServer] event arguments.
    /// </summary>
    /// Note: To be removed
    public class GetMessageFromServerEventArgs : EventArgs, IGetMessageFromServerEventArgs
    {
        #region Constructors

        public GetMessageFromServerEventArgs(string Msg, CommandId cmdId)
        {
            this.Message = Msg;
            this.CmdId = cmdId;
        }

        #endregion Constructors

        #region Properties

        public CommandId CmdId { get; }
        public string Message { get; }

        #endregion Properties
    }

    /// <summary>
    /// [OperationDone] event arguments.
    /// </summary>
    public class OperationDoneEventArgs : EventArgs, IOperationDoneEventArgs
    {
        #region Constructors

        public OperationDoneEventArgs(CommandId cmdId, bool result)
        {
            this.CmdId = cmdId;
            this.Result = result;
        }

        #endregion Constructors

        #region Properties

        public CommandId CmdId { get; }
        public bool Result { get; }

        #endregion Properties
    }
}
