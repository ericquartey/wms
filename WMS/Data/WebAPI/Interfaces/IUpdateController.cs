using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IUpdateController<TModel, in TKey>
    {
        #region Methods

        Task<ActionResult<TModel>> UpdateAsync(TModel model, TKey id);

        #endregion
    }
}
