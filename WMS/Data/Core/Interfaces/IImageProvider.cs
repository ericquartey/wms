using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IImageProvider
    {
        #region Methods

        ImageFile GetById(string key);

        Task<string> CreateAsync(IFormFile model);

        #endregion
    }
}
