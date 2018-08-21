using Ferretto.Common.Models;

namespace Ferretto.Common.BLL.Interfaces.Models
{
  public interface IItem : IModel<int>
  {
    string Code { get; set; }
    string Description { get; set; }
    AbcClass AbcClass { get; set; }
    MeasureUnit MeasureUnit { get; set; }
    int? Width { get; set; }
    int? Length { get; set; }
    int? Height { get; set; }
    string Image { get; set; }
  }
}
