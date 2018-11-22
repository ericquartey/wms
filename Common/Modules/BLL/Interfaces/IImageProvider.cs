using System.Windows.Media;

namespace Ferretto.Common.Modules.BLL
{
    public interface IImageProvider
    {
        #region Methods

        ImageSource GetImage(string pathName);

        #endregion Methods
    }
}
