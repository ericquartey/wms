using System.Windows.Media;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IImageProvider
    {
        #region Methods

        ImageSource GetImage(string pathName);

        #endregion Methods
    }
}
