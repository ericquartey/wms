using System.IO;
using Microsoft.AspNetCore.Http;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "Ok",
        Scope = "member",
        Target = "~P:Ferretto.Common.BLL.Interfaces.Models.IImageFile.FileBytes")]
    public interface IImageFile : IModel<string>, IFormFile
    {
        #region Properties

        byte[] FileBytes { get; set; }

        Stream Stream { get; set; }

        #endregion
    }
}
