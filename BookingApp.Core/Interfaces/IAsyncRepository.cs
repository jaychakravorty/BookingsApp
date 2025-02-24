using BookingApp.Core.Domain.Models;
using System.Linq.Expressions;

namespace BookingApp.Core.Interfaces
{
    public interface IAsyncRepository
    {
        Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseModel;

        Task<IList<T>> BulkAddAsync<T>(IList<T> entity, CancellationToken cancellationToken) where T : BaseModel;

        Task DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T: BaseModel;

        Task<IList<T>> GetAllAsNoTrackAsync<T>(CancellationToken cancellationToken) where T : BaseModel;

        Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate, bool asTracking, CancellationToken cancellationToken) where T : BaseModel;

        Task CleanAllTables(string sqlScript);

    }
}
