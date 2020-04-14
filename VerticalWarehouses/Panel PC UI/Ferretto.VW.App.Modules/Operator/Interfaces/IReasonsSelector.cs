using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator
{
    public interface IOperationReasonsSelector
    {
        #region Properties

        int? ReasonId { get; set; }

        string ReasonNotes { get; set; }

        IEnumerable<OperationReason> Reasons { get; }

        ICommand CancelReasonCommand { get; }

        ICommand ConfirmReasonCommand { get; }

        #endregion

        #region Methods

        Task<bool> CheckReasonsAsync();

        #endregion
    }
}
