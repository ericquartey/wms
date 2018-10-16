using System;

namespace Ferretto.Common.Controls
{
    public class EventUI
    {
        #region Fields

        public object Data;

        #endregion Fields

        #region Constructors

        public EventUI(object sender, EventArgs eventArgs, object data)
        {
            this.Sender = sender;
            this.EventUIArgs = eventArgs;
            this.Data = data;
        }

        #endregion Constructors

        #region Properties

        public EventArgs EventUIArgs { get; set; }
        public object Sender { get; set; }

        #endregion Properties
    }
}
