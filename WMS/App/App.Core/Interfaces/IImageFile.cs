using System.IO;
using Ferretto.Common.BLL.Interfaces.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IImageFile : IModel<string>, IFormFile
    {
        #region Properties

        Stream Stream { get; set; }

        #endregion
    }
}
