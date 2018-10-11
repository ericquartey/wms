using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ferretto.Common.Controls
{
    public class EventUI
    {
        public EventArgs eventArgs { get; set; }
        public object sender { get; set; }
        public object data;

        public EventUI(object sender, EventArgs eventArgs, object data)
        {
            this.sender = sender;
            this.eventArgs = eventArgs;
            this.data = data;
        }
    }
}
