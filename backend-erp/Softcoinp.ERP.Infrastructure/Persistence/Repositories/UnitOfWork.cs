using Softcoinp.ERP.Domain.Interfaces;
using System.Collections.Concurrent;

namespace Softcoinp.ERP.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<string, object>();
    }

    public IGenericRepository<T> GetRepository<T>() where T : class
    {
        var typeName = typeof(T).Name;

        return (IGenericRepository<T>)_repositories.GetOrAdd(typeName, _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
