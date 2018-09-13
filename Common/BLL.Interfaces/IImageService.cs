using System.Windows.Media;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IImageService
    {
        ImageSource GetImage(string pathName);
    }
}
