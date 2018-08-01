namespace Ferretto.Common.BLL.Interfaces
{
  public interface IEventService
  {
    void Invoke<T>(T eventArgs) where T : IEventArgs;
  }
}
