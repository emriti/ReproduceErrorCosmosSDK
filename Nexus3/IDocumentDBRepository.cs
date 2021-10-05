using Microsoft.Azure.EventGrid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nexus.Base.CosmosDBRepository
{
    public interface IDocumentDBRepository<T>: IDisposable where T :  ModelBase
    {
        string GetDatabaseName();
        string GetCollectionName();
        string ExtractPartitionKey(T item);
        Task ChangeThroughput(int throughput);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task<T> CreateAsync(T item, EventGridOptions options = null, string createdBy = null, string activeFlag = null);
        Task DeleteAllAsync();
        Task DeleteAllAsync(Dictionary<string, string> partitionKey = null);
        Task DeleteAsync(string id, Dictionary<string, string> partitionKeys = null, EventGridOptions options = null);
        void Dispose();
        Task<PageResult<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, T>> selector = null, bool usePaging = false, string continuationToken = null, int pageSize = 10, Dictionary<string, string> partitionKeys = null);
        Task<IEnumerable<T>> GetAsync(int pageNumber, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Expression<Func<T, bool>> predicate = null, Expression<Func<T, T>> selector = null, int pageSize = 10, Dictionary<string, string> partitionKeys = null);
        Task<PageResult<T>> GetAsync(string sqlQuery, bool usePaging = false, string continuationToken = null, int pageSize = 10, Dictionary<string, string> partitionKeys = null);
        Task<T> GetByIdAsync(string id, Dictionary<string, string> partitionKeys = null);
        Task RepublishAsync();
        Task<IEnumerable<SynchronizationException>> Synchronize(IEnumerable<EventGridEvent> eventData);
        Task<T> UpdateAsync(string id, T item, EventGridOptions options = null, string lastUpdatedBy = null);
        Task<T> UpsertAsync(string id, T item, EventGridOptions options = null, bool manuallyManageIStandardWhoPropertyValue = true, string createdBy = null, string lastUpdatedBy = null, string activeFlag = null);
    }
}