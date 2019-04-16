using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IImageProvider
    {
        #region Methods

        Task<IOperationResult<string>> CreateAsync(IFormFile model);

        ImageFile GetById(string key);

        #endregion
    }
}
