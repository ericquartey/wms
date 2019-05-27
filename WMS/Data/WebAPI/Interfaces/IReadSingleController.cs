using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IReadSingleController<TModel, in TKey>
    {
        #region Methods

        Task<ActionResult<TModel>> GetByIdAsync(TKey id);

        #endregion
    }
}
