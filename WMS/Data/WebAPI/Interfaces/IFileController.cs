using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Interfaces
{
    public interface IFileController
    {
        #region Methods

        ActionResult Download(string id);

        Task<ActionResult<string>> UploadAsync(IFormFile model);

        #endregion
    }
}
