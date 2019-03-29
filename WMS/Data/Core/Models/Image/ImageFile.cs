using System.IO;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ImageFile
    {
        #region Properties

        public string ContentType { get; set; }

        public string Path { get; set; }

        public Stream Stream { get; set; }

        #endregion
    }
}
