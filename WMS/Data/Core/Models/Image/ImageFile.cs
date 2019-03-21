using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "Ok",
        Scope = "member",
        Target = "~P:Ferretto.Common.BLL.Interfaces.Models.IImageFile.FileBytes")]
    public class ImageFile : IImageFile
    {
        #region Properties

        public string ContentDisposition { get; set; }

        public string ContentType { get; set; }

        public byte[] FileBytes { get; set; }

        public string FileName { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public string Id { get; set; }

        public long Length { get; set; }

        public string Name { get; set; }

        public Stream Stream { get; set; }

        #endregion

        #region Methods

        public void CopyTo(Stream target)
        {
            this.Stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.Stream.CopyToAsync(target);
        }

        public Stream OpenReadStream()
        {
            return this.Stream;
        }

        #endregion
    }
}
