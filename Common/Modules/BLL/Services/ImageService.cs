using System;
using System.Windows.Media;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Services
{
  public class ImageService : IImageService
  {
    #region Fields

    private readonly IImageRepository imageRepository;

    #endregion Fields

    #region Constructors

    public ImageService(IImageRepository imageRepository)
    {
      this.imageRepository = imageRepository;
    }

    #endregion Constructors

    #region Methods

    public ImageSource GetImage(string pathName)
    {
      if (String.IsNullOrWhiteSpace(pathName))
      {
        throw new ArgumentException(Errors.ParameterCannotBeNullOrWhitespace, nameof(pathName));
      }

      return this.imageRepository.GetById(pathName);
    }

    #endregion Methods
  }
}
