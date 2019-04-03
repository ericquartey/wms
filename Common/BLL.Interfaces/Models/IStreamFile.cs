using System.IO;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IStreamFile : IModel<string>
    {
        #region Properties

        Stream Stream { get; set; }

        #endregion
    }
}
