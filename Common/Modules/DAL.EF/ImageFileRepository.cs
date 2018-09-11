using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.DAL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.DAL.EF
{
  // TODO implement CRUD operations (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/140)
  public class ImageFileRepository : IImageRepository
  {
    #region Fields

    // TODO make the images directory configurable (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/139)
    private const string imagesDirectoryName = "images\\";

    #endregion Fields

    #region Properties

    private static Uri ImageDirectoryUri => new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, imagesDirectoryName));

    #endregion Properties

    #region Methods

    public void Delete(ImageSource entity)
    {
      throw new NotSupportedException();
    }

    public ImageSource GetById(string id)
    {
      var uri = new Uri(ImageDirectoryUri, id);

      if (!ImageDirectoryUri.IsBaseOf(uri))
      {
        throw new ArgumentException(
          Errors.SpecifiedPathNotInConfiguredImageFolder,
          nameof(id));
      }

      return new BitmapImage(uri);
    }

    public ImageSource Insert(ImageSource entity)
    {
      throw new NotSupportedException();
    }

    public IEnumerable<ImageSource> List()
    {
      throw new NotSupportedException();
    }

    public void Update(ImageSource entity)
    {
      throw new NotSupportedException();
    }

    #endregion Methods
  }
}
