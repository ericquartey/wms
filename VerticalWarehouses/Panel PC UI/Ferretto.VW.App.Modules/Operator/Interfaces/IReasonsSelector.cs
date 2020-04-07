using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IOperationReasonsSelector
    {
        #region Properties

        int? ReasonId { get; set; }

        string ReasonNotes { get; set; }

        IEnumerable<OperationReason> Reasons { get; }

        #endregion

        #region Methods

        Task<bool> CheckReasonsAsync();

        #endregion
    }
}
