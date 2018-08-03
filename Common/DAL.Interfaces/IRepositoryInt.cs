namespace Ferretto.Common.DAL.Interfaces
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Minor Code Smell",
    "S4023:Interfaces should not be empty",
    Justification = "The interface is used to identify a set of types at compile time.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Minor Code Smell",
    "AC1040",
    Justification = "The interface is used to identify a set of types at compile time.")]
  public interface IRepositoryInt<TEntity> : IRepository<TEntity, int> where TEntity : class
  {
    
  }
}
