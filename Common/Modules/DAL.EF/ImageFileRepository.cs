using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.Common.DAL.Interfaces;

namespace Ferretto.Common.Modules.DAL.EF
{
  // TODO implement CRUD operations (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/140)
  public class ImageFileRepository : IImageRepository
  {
    // TODO make the images directory configurable (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/139)
    private const string imagesDirectoryName = "images\\";

    private static Uri ImageDirectoryUri
    {
      get
      {
        return new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, imagesDirectoryName));
      }
    }

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
          "The specified path is not contained in the image directory.",
          nameof(id));
      }

      return new BitmapImage(uri);
    }

    public void Insert(ImageSource entity)
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
  }
}
