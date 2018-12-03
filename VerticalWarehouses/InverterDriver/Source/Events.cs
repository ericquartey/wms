using System;

namespace Ferretto.VW.InverterDriver
{
    /// <summary>
    ///Delegate for the [Connected] event.
    /// </summary>
    public delegate void ConnectedEventHandler(object sender, ConnectedEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [EnquiryTelegramDone] event.
    /// </summary>
    public delegate void EnquiryTelegramDoneEventHandler(object sender, EnquiryTelegramDoneEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [Error] event.
    /// </summary>
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [LastRequestDone] event.
    /// </summary>
    public delegate void LastRequestDoneEventHandler(object sender, LastRequestDoneEventArgs eventArgs);

    /// <summary>
    /// Delegate for the [SelectTelegramDone] event.
    /// </summary>
    public delegate void SelectTelegramDoneEventHandler(object sender, SelectTelegramDoneEventArgs eventArgs);

    /// <summary>
    /// [Connected] event arguments.
    /// </summary>
    public class ConnectedEventArgs : EventArgs, IConnectedEventArgs
    {
        #region Constructors

        public ConnectedEventArgs(bool State)
        {
            this.State = State;
        }

        #endregion Constructors

        #region Properties

        public bool State { get; }

        #endregion Properties
    }

    /// <summary>
    /// [EnquiryTelegramDone] event arguments.
    /// </summary>
    public class EnquiryTelegramDoneEventArgs : EventArgs, IEnquiryTelegramDoneEventArgs
    {
        #region Constructors

        public EnquiryTelegramDoneEventArgs(ParameterID paramID, object value, ValueDataType type)
        {
            this.ParamID = paramID;
            this.Value = value;
            this.Type = type;
        }

        #endregion Constructors

        #region Properties

        public ParameterID ParamID { get; }

        public ValueDataType Type { get; }
        public object Value { get; }

        #endregion Properties
    }

    /// <summary>
    /// [Error] event arguments.
    /// </summary>
    public class ErrorEventArgs : EventArgs, IErrorEventArgs
    {
        #region Constructors

        public ErrorEventArgs(InverterDriverErrors error)
        {
            this.ErrorCode = error;
        }

        #endregion Constructors

        #region Properties

        public InverterDriverErrors ErrorCode { get; }

        #endregion Properties
    }

    /// <summary>
    /// [LastRequestDone] event arguments.
    /// </summary>
    public class LastRequestDoneEventArgs : EventArgs, ILastRequestDoneEventArgs
    {
        #region Constructors

        public LastRequestDoneEventArgs(bool state)
        {
            this.State = state;
        }

        #endregion Constructors

        #region Properties

        public bool State { get; }

        #endregion Properties
    }

    /// <summary>
    /// [SelectTelegramDone] event arguments.
    /// </summary>
    public class SelectTelegramDoneEventArgs : EventArgs, ISelectTelegramDoneEventArgs
    {
        #region Constructors

        public SelectTelegramDoneEventArgs(ParameterID paramID, object value, ValueDataType type)
        {
            this.ParamID = paramID;
            this.Value = value;
            this.Type = type;
        }

        #endregion Constructors

        #region Properties

        public ParameterID ParamID { get; }

        public ValueDataType Type { get; }
        public object Value { get; }

        #endregion Properties
    }
}
