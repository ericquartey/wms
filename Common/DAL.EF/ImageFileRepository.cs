using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ferretto.Common.DAL.EF
{
  // TODO implement CRUD operations (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/140)
  public class ImageFileRepository : Interfaces.IImageRepository
  {
    // TODO make the images directory configurable (https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/139)
    private static readonly string imagesDirectoryName = "images\\"; 

    private Uri ImageDirectoryUri
    {
      get
      {
        return new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, imagesDirectoryName));
      }
    }

    public void Delete(ImageSource entity)
    {
      throw new NotImplementedException(); 
    }

    public ImageSource GetById(string id)
    {
      var uri = new Uri(this.ImageDirectoryUri, id);
      
      if (!this.ImageDirectoryUri.IsBaseOf(uri))
      {
        throw new ArgumentException(
          "The specified path is not contained in the image directory.",
          nameof(id));
      }

      return new BitmapImage(uri);
    }

    public void Insert(ImageSource entity)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<ImageSource> List()
    {
      throw new NotImplementedException();
    }

    public void Update(ImageSource entity)
    {
      throw new NotImplementedException();
    }
  }
}
