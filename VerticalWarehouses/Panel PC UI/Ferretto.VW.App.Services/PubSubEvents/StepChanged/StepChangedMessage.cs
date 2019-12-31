using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class StepChangedMessage
    {
        #region Constructors

        public StepChangedMessage(bool next)
        {
            this.Next = next;
        }

        #endregion

        #region Properties

        public bool Next { get; }

        #endregion
    }
}
