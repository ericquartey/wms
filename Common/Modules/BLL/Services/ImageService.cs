using System;
using System.Windows.Media;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Modules.BLL.Services
{
  public class ImageService : IImageService
  {
    private readonly IImageRepository imageRepository;

    public ImageService(IImageRepository imageRepository)
    {
      this.imageRepository = imageRepository;
    }

    public ImageSource GetImage(string pathName)
    {
      if (String.IsNullOrWhiteSpace(pathName))
      {
        throw new ArgumentException("The parameter cannot be null or whitespace.", nameof(pathName));
      }

      return this.imageRepository.GetById(pathName);
    }
  }
}
