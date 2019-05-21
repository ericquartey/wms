﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IItemProvider :
        IPagedBusinessProvider<Item, int>,
        ICreateAsyncProvider<ItemDetails, int>,
        IReadSingleAsyncProvider<ItemDetails, int>,
        IUpdateAsyncProvider<ItemDetails, int>,
        IDeleteAsyncProvider<ItemDetails, int>,
        IAllowedByLoadingUnitProvider<Item, int>
    {
        #region Methods

        Task<IEnumerable<AllowedItemInCompartment>> GetAllowedByCompartmentIdAsync(int compartmentId);

        Task<ItemDetails> GetNewAsync();

        Task<IOperationResult<SchedulerRequest>> PickAsync(ItemPick itemPick);

        Task<IOperationResult<SchedulerRequest>> PutAsync(ItemPut itemPut);

        #endregion
    }
}
