using System;

namespace Ferretto.VW.Drivers.Inverter
{
    /// <summary>
    /// [SelectTelegramDone] event arguments.
    /// </summary>
    public class SelectTelegramDoneEventArgs : EventArgs, ISelectTelegramDoneEventArgs
    {
        #region Constructors

        public SelectTelegramDoneEventArgs(ParameterId parameterId, object value, ValueDataType type)
        {
            this.ParamId = parameterId;
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
