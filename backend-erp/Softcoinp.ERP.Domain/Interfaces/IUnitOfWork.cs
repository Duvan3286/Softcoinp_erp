using System;
using System.Threading.Tasks;

namespace Softcoinp.ERP.Domain.Interfaces;

/// <summary>
/// Interface for Unit of Work pattern to manage transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> GetRepository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task<int> CompleteAsync();
}
