using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_AutomationService.Interfaces
{
    public interface IOperatorHub
    {
        #region Methods

        Task SetBayDrawerOperationToInventory();

        Task SetBayDrawerOperationToPick();

        Task SetBayDrawerOperationToRefill();

        Task SetBayDrawerOperationToWaiting();

        #endregion
    }
}
