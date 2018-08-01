namespace Ferretto.Common.DAL.Interfaces
{
  public interface IRepositoryInt<TEntity> : IRepository<TEntity, int> where TEntity : class
  {
    
  }
}
