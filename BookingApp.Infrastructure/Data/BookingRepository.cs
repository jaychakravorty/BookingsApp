using BookingApp.Core.Domain.Models;
using BookingApp.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingApp.Infrastructure.Data
{
    public class BookingRepository : IAsyncRepository
    {
        private readonly BookingContext _bookingContext;
       
        public BookingRepository(BookingContext bookingContext)
        {
            this._bookingContext = bookingContext ?? throw new ArgumentNullException(nameof(bookingContext));
                
        }
        public async Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseModel
        {
            try
            {
                await this._bookingContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
                await this._bookingContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return entity;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IList<T>> BulkAddAsync<T>(IList<T> entity, CancellationToken cancellationToken) where T : BaseModel
        {
            await this._bookingContext.AddRangeAsync(entity, cancellationToken).ConfigureAwait(false);
            await this._bookingContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }

        public async Task DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseModel
        {
            this._bookingContext.Remove(entity);
            await this._bookingContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public  async Task<IList<T>> GetAllAsNoTrackAsync<T>(CancellationToken cancellationToken) where T : BaseModel
        {
            return await this._bookingContext.Set<T>().AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate, bool asTracking, CancellationToken cancellationToken) where T : BaseModel
        {
            IQueryable<T> query = this._bookingContext.Set<T>();

            // Dynamically include navigation properties if they exist
            var navigationProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string));

            foreach (var navigationProperty in navigationProperties)
            {
                query = query.Include(navigationProperty.Name);
            }

            query = asTracking ? query.Where(predicate) : query.AsNoTracking().Where(predicate);

            var entity = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return entity.FirstOrDefault();
        }

        public async Task CleanAllTables(string sqlScript)
        {
            await _bookingContext.Database.ExecuteSqlRawAsync(sqlScript).ConfigureAwait(false);

        }
    }
}
