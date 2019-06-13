using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IFeedbackNotifier
    {
        #region Methods

        void Notify(string s);

        #endregion
    }
}
