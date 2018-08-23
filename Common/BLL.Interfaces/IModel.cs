namespace Ferretto.Common.BLL.Interfaces
{
  public interface IModel<TId>
  {
    TId Id { get; set; }
  }
}
