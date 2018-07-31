using System.Windows.Media;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.BLL.Services
{
  public class ImageService : Interfaces.IImageService
  {
    private readonly IImageRepository imageRepository;

    public ImageService(IImageRepository imageRepository)
    {
      this.imageRepository = imageRepository;
    }

    public ImageSource GetImage(string pathName)
    {
      return imageRepository.GetById(pathName);
    }
  }
}
