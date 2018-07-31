using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ferretto.Common.DAL.EF
{
  // TODO not yet implemented (class set as abstract to avoid instantiation)
  // see https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/141
  public abstract class ImageDbRepository : Interfaces.IImageRepository
  {
    public void Delete(ImageSource entity)
    {
      throw new NotImplementedException(); 
    }

    public ImageSource GetById(string id)
    {
      throw new NotImplementedException();
    }

    public void Insert(ImageSource entity)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<ImageSource> List()
    {
      throw new NotImplementedException();
    }

    public void SaveChanges()
    {
      throw new NotImplementedException();
    }

    public void Update(ImageSource entity)
    {
      throw new NotImplementedException();
    }
  }
}
