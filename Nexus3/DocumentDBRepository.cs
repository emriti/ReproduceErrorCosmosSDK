using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using Nexus.Base.EventGridExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Nexus.Base.CosmosDBRepository
{
    /// <summary>
    /// Document DB Repository base class
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public class DocumentDBRepository<T> : IDocumentDBRepository<T> where T : ModelBase
    {
        /// <summary>
        /// CosmosDB database id
        /// </summary>
        private string _databaseId;

        /// <summary>
        /// Default page size whenever page is not required
        /// Fix bugs to MaxItemCount -1 in Cosmos that return 1 data instead of dynamic data
        /// </summary>
        private int _defaultPageSize = -1;

        /// <summary>
        /// CosmosDB database id
        /// </summary>
        public string GetDatabaseName() => _databaseId;

        /// <summary>
        /// CosmosDB collection id
        /// </summary>
        private string _collectionId = ModelBase.GenerateDocumentType(typeof(T));

        /// <summary>
        /// CosmosDB database id
        /// </summary>
        public string GetCollectionName() => ModelBase.GenerateDocumentType(typeof(T));

        /// <summary>
        /// Redis Cache Prefix Key to be appended with document id
        /// </summary>
        private string _cacheKeyPrefix = "";

        /// <summary>
        /// CosmosDB client connection handler 
        /// </summary>
        private Lazy<CosmosClient> _client;

        /// <summary>
        /// Event grid end point
        /// </summary>
        private string _eventGridEndpoint;

        /// <summary>
        /// Event grid key
        /// </summary>
        private string _eventGridKey;

        /// <summary>
        /// Additional partition key beside collection name
        /// </summary>
        private bool _partitionPropertyDefined;
        private List<string> _partitionPropertyNames = null;
        private List<PropertyInfo> _partitionProperties = new List<PropertyInfo>();


        /// <summary>
        /// Disposed flag
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Model namespace
        /// </summary>
        private string _namespace = ModelBase.GenerateDocumentNamespace(typeof(T));

        #region "Constructor and Overrides"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="databaseId">Database Id</param>
        /// <param name="cosmosDBClient">CosmosDB client handler</param>
        /// <param name="cacheConnection">Cache connection</param>
        /// <param name="cacheKeyPrefix">Cache key-prefix</param>
        /// <param name="eventGridEndPoint">EventGrid end point</param>
        /// <param name="eventGridKey">EventGrid key</param>
        /// <param name="partitionProperties">Partition property</param>
        /// <param name="createDatabaseIfNotExist">Create database & collection when it's not exist</param>
        public DocumentDBRepository(string databaseId,
            CosmosClient cosmosDBClient,
            string cacheConnection = "", string cacheKeyPrefix = "",
            string eventGridEndPoint = "", string eventGridKey = "",
            string partitionProperties = "", bool createDatabaseIfNotExist = false)
        {
            if (_client == null)
            {
                _client = new Lazy<CosmosClient>(cosmosDBClient);
            }

            SetUp(databaseId, "", "", createDatabaseIfNotExist, cacheConnection, cacheKeyPrefix,
                 eventGridEndPoint, eventGridKey, partitionProperties);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="databaseId">Database Id</param>
        /// <param name="endPoint">Database end-point</param>
        /// <param name="key">Database key</param>
        /// <param name="cacheConnection">Cache connection</param>
        /// <param name="cacheKeyPrefix">Cache key-prefix</param>
        /// <param name="eventGridEndPoint">EventGrid end-point</param>
        /// <param name="eventGridKey">EventGrid key</param>
        /// <param name="partitionProperties">Partition property</param>
        /// <param name="createDatabaseIfNotExist">Create database & collection when it's not exist</param>
        public DocumentDBRepository(string databaseId,
          string endPoint, string key,
          string cacheConnection = "", string cacheKeyPrefix = "",
          string eventGridEndPoint = "", string eventGridKey = "",
          string partitionProperties = "", bool createDatabaseIfNotExist = false)
        {
            if (string.IsNullOrEmpty(endPoint))
                throw new ArgumentException("Invalid Document Database End Point", "endPoint");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Invalid Document Database Key", "key");

            SetUp(databaseId, endPoint, key, createDatabaseIfNotExist, cacheConnection, cacheKeyPrefix,
                     eventGridEndPoint, eventGridKey, partitionProperties);
        }

        private void SetUp(string databaseId,
            string endPoint, string key, bool createDatabaseIfNotExist,
            string cacheConnection = "", string cacheKeyPrefix = "",
            string eventGridEndPoint = "", string eventGridKey = "",
            string partitionProperties = "")
        {
            _databaseId = databaseId;

            if (string.IsNullOrEmpty(_databaseId))
                throw new ArgumentException("Invalid Database Id", "databaseId");
            if (string.IsNullOrEmpty(_collectionId))
                throw new ArgumentException("Invalid Collection Id", "collectionId");

            if (!string.IsNullOrEmpty(partitionProperties))
            {
                _partitionPropertyDefined = true;

                _partitionPropertyNames = partitionProperties.Split(",").ToList();

                foreach (var propertyName in _partitionPropertyNames)
                {
                    _partitionProperties.Add(typeof(T).GetProperty(propertyName));
                }
            }

            if (endPoint != "" && key != "")
            {
                if (ConnectionHandler.DocumentHandler.ContainsKey(endPoint))
                {
                    _client = new Lazy<CosmosClient>(ConnectionHandler.DocumentHandler[endPoint]);
                }
                else
                {
                    _client = new Lazy<CosmosClient>(
                        ConnectionHandler.DocumentHandler.GetOrAdd(endPoint,
                        new CosmosClient(endPoint,
                        key,
                        new CosmosClientOptions()
                        {
                            IdleTcpConnectionTimeout = new TimeSpan(0, 20, 0),
                            PortReuseMode = PortReuseMode.PrivatePortPool,
                            MaxRetryAttemptsOnRateLimitedRequests = 10,
                            MaxRetryWaitTimeOnRateLimitedRequests = new TimeSpan(0, 1, 0),
                            RequestTimeout = new TimeSpan(0, 5, 0)
                        }))
                    );
                }

                if (createDatabaseIfNotExist)
                {
                    CreateDatabaseIfNotExistsAsync().Wait();

                    if (!ConnectionHandler.RegisteredCollections.Contains(endPoint + "||" + _databaseId + "||" + _collectionId))
                    {
                        CreateCollectionIfNotExistsAsync().Wait();
                        ConnectionHandler.RegisteredCollections.Add(endPoint + "||" + _databaseId + "||" + _collectionId);
                    }
                }
            }
            else if (_client != null)
            {
                if (createDatabaseIfNotExist)
                {
                    CreateDatabaseIfNotExistsAsync().Wait();
                    CreateCollectionIfNotExistsAsync().Wait();
                }
            }
            else
            {
                throw new ArgumentException("Invalid CosmosDB Client", "cosmosDBClient");
            }

            if (!string.IsNullOrEmpty(cacheConnection))
            {
                _cacheKeyPrefix = (cacheKeyPrefix != "") ? cacheKeyPrefix : "[" + ModelBase.GenerateDocumentType(typeof(T)) + "]:";

            }

            if (!string.IsNullOrEmpty(eventGridEndPoint) && !string.IsNullOrEmpty(eventGridKey))
            {
                _eventGridEndpoint = eventGridEndPoint;

                _eventGridKey = eventGridKey;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //_client.Dispose();
                }
            }
            _disposed = true;
        }

        #endregion

        #region "Public"

        public virtual async Task<T> GetByIdAsync(string id, Dictionary<string, string> partitionKeys = null)
        {
            string partitionKey = ComposePartitionKeys(partitionKeys);
            return await GetByIdAsync(id, partitionKey);
        }

        /// <summary>
        /// Get data by document id
        /// </summary>
        /// <param name="id">Id to be search.</param>
        /// <returns>Document</returns>
        private async Task<T> GetByIdAsync(string id, string partitionKey = "")
        {
            if (_cacheKeyPrefix != "")
            {
                //var cacheResult = await CacheManager.GetObject<T>(_cacheKeyPrefix + id);
                var cacheResult = await CacheManager.GetObject<string>(_cacheKeyPrefix + id);
                if (cacheResult != null)
                {
                    return JsonConvert.DeserializeObject<T>(cacheResult);
                    //return cacheResult;
                }
            }

            try
            {
                var client = _client.Value;
                var container = client.GetContainer(_databaseId, _collectionId);

                var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

                var dbresult = response?.Resource;

                if (dbresult != null && !dbresult.DocumentNamespace.StartsWith(_namespace))
                    return null;

                if (_cacheKeyPrefix != "")
                {
                    //await CacheManager.SetObject(_cacheKeyPrefix + id, dbresult);
                    await CacheManager.SetObject(_cacheKeyPrefix + id, JsonConvert.SerializeObject(response));
                }

                return response;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate = null)
        {
            var client = _client.Value;
            var container = client.GetContainer(_databaseId, _collectionId);

            var queryOptions = new QueryRequestOptions { MaxItemCount = -1 };

            if (predicate == null)
            {
                predicate = p => true;
            }

            return await container.GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Where(predicate).Where(p => p.DocumentNamespace.Contains(_namespace))
                .CountAsync();
        }

        /// <summary>
        /// Get data based on sql query
        /// </summary>
        /// <param name="predicate">Search criteria</param>
        /// <param name="orderBy">Order predicate</param>
        /// <param name="usePaging">Indicate to use paging</param>
        /// <param name="continuationToken">Token that indicate current page position</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        public async Task<PageResult<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Expression<Func<T, T>> selector = null,
            bool usePaging = false, string continuationToken = null, int pageSize = 10,
            Dictionary<string, string> partitionKeys = null)
        {
            var client = _client.Value;
            var container = client.GetContainer(_databaseId, _collectionId);
            PartitionKey? pk = null;
            if (partitionKeys != null) pk = new PartitionKey(ComposePartitionKeys(partitionKeys));

            var maxItemCount = partitionKeys == null ? _defaultPageSize : -1;

            var queryReqOpts = new QueryRequestOptions
            {
                MaxItemCount = usePaging ? pageSize : maxItemCount,
                PartitionKey = pk
            };

            predicate ??= p => true;

            if (usePaging) orderBy ??= o => o.OrderBy(p => p.Id);

            var query = container.GetItemLinqQueryable<T>(
                    continuationToken: continuationToken != "" ? continuationToken : null,
                    requestOptions: queryReqOpts)
                .Where(predicate).Where(p => p.DocumentNamespace.Contains(_namespace));

            if (orderBy != null)
            {
                query = orderBy(query).Select(p => p);
            }

            if (selector != null)
            {
                query = query.Select(selector);
            }

            var results = new List<T>();
            FeedResponse<T> response;
            using (var feedIterator = query.ToFeedIterator())
            {
                if (usePaging)
                {
                    response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                    return new PageResult<T>(results, response.ContinuationToken, response.RequestCharge);
                }
                else
                {
                    var requestCharge = 0.0;
                    while (feedIterator.HasMoreResults)
                    {
                        response = await feedIterator.ReadNextAsync();
                        results.AddRange(response);
                        requestCharge += response.RequestCharge;
                    }

                    return new PageResult<T>(results, "", requestCharge);
                }
            }
        }

        /// <summary>
        /// Get data by page number
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="predicate">Search criteria</param>
        /// <param name="orderBy">Order predicate</param>
        /// <param name="selector">Item to be selected</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAsync(int pageNumber,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, T>> selector = null, int pageSize = 10,
            Dictionary<string, string> partitionKeys = null)
        {
            var result = new PageResult<T>(null, "");

            for (int ii = 0; ii < pageNumber; ii++)
            {
                result = await GetAsync(predicate, orderBy, selector, true,
                    result.ContinuationToken, pageSize, partitionKeys);

                if (result.ContinuationToken == null && ii != pageNumber - 1)
                    return new List<T>();
            }

            return result.Items;
        }

        /// <summary>
        /// Get data based on sql query. 
        /// </summary>
        /// <param name="sqlQuery">SQL Query</param>
        /// <param name="usePaging">Indicate to use paging</param>
        /// <param name="continuationToken">Token that indicate current page position</param>
        /// <param name="pageSize">Number of item per-page</param>
        /// <returns>List of document</returns>
        /// <remarks>Caller should manually specify documentType to minimize RU.</remarks>
        public async Task<PageResult<T>> GetAsync(string sqlQuery,
           bool usePaging = false, string continuationToken = null, int pageSize = 10,
           Dictionary<string, string> partitionKeys = null)
        {

            var client = _client.Value;
            var container = client.GetContainer(_databaseId, _collectionId);
            PartitionKey? pk = null;
            if (partitionKeys != null) pk = new PartitionKey(ComposePartitionKeys(partitionKeys));

            var maxItemCount = partitionKeys == null ? _defaultPageSize : -1;

            var queryReqOpts = new QueryRequestOptions
            {
                MaxItemCount = usePaging ? pageSize : maxItemCount,
                PartitionKey = pk
            };

            var results = new List<T>();
            FeedResponse<T> response;
            using (var feedIterator = container.GetItemQueryIterator<T>(sqlQuery, continuationToken != "" ? continuationToken : null, queryReqOpts))
            {
                if (usePaging)
                {
                    response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                    return new PageResult<T>(results, response.ContinuationToken, response.RequestCharge);
                }
                else
                {
                    var requestCharge = 0.0;
                    while (feedIterator.HasMoreResults)
                    {
                        response = await feedIterator.ReadNextAsync();
                        results.AddRange(response);
                        requestCharge += response.RequestCharge;
                    }

                    return new PageResult<T>(results, "", requestCharge);
                }
            }
        }

        /// <summary>
        /// Create new document
        /// </summary>
        /// <param name="item">Document to be created</param>
        /// <param name="options">Eventgrid behavior options</param>
        /// <param name="createdBy">Creator user name</param>
        /// <param name="activeFlag">active Flag status on record creation</param>
        /// <returns>Newly created document</returns>
        public async Task<T> CreateAsync(T item, EventGridOptions options = null,
            string createdBy = null, string activeFlag = null)
        {
            var client = _client.Value;
            var container = client.GetContainer(_databaseId, _collectionId);

            item.CreatedBy = item.LastUpdatedBy = createdBy;
            item.CreatedDate = item.LastUpdatedDate = DateTime.UtcNow;
            item.ActiveFlag = activeFlag ?? "Y";
            item.PartitionKey = GeneratePartitionKey(item);

            if (item.Id == null)
                item.Id = Guid.NewGuid().ToString();

            var createdItem = await container.CreateItemAsync(item);

            var eventGridSubject = "Create/";
            var shouldPublishEvent = true;

            if (options != null)
            {
                eventGridSubject = options.Subject;
                shouldPublishEvent = options.PublishEvent;
            }

            if (_eventGridEndpoint != null && shouldPublishEvent)
            {
                var eventData = new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    DataVersion = "1.0",
                    EventTime = DateTime.UtcNow,
                    Subject = eventGridSubject,
                    EventType = typeof(T).ToString(),
                    Data = JsonConvert.SerializeObject(item)
                };

                await eventData.PublishAsync(_eventGridEndpoint, _eventGridKey);
            }

            return createdItem;
        }

        /// <summary>
        /// Republish every row in database to Event Grid
        /// </summary>
        /// <returns></returns>
        public async Task RepublishAsync()
        {
            if (_eventGridEndpoint == null)
                return;

            var result = new PageResult<T>(null, "");

            while (result.ContinuationToken != null)
            {
                result = await GetAsync(usePaging: true, pageSize: 50, continuationToken: result.ContinuationToken);

                foreach (var item in result.Items)
                {
                    var eventData = new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        DataVersion = "1.0",
                        EventTime = DateTime.UtcNow,
                        Subject = "Republish/",
                        EventType = typeof(T).ToString(),
                        Data = JsonConvert.SerializeObject(item)
                    };

                    await eventData.PublishAsync(_eventGridEndpoint, _eventGridKey);
                }
            }
        }

        /// <summary>
        /// Synchronize data from another service
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SynchronizationException>> Synchronize(IEnumerable<EventGridEvent> eventData)
        {
            var exceptions = new List<SynchronizationException>();

            foreach (var currentEvent in eventData)
            {
                try
                {
                    var item = currentEvent.GetData<T>();

                    switch (currentEvent.Subject)
                    {
                        case "Create/":
                            await CreateAsync(item);
                            break;
                        case "Update/":
                            await UpdateAsync(item.Id, item);
                            break;
                        case "Upsert/":
                        case "Republish/":
                            await UpsertAsync(item.Id, item);
                            break;
                        case "Delete/":
                            await DeleteAsync(item.Id);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(new SynchronizationException() { Exception = ex, EventGridEvent = currentEvent });
                }
            }

            return exceptions;
        }

        /// <summary>
        /// Generate partition key
        /// </summary>
        private string GeneratePartitionKey(T item)
        {
            var result = _collectionId;

            if (_partitionPropertyNames != null)
            {
                foreach (var p in _partitionProperties)
                {
                    result += "/" + p.GetValue(item);
                }
            }

            return result;
        }

        public string ExtractPartitionKey(T item) => GeneratePartitionKey(item);

        /// <summary>
        /// Update document
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="item">Document to be updated</param>
        /// <param name="options">Eventgrid behavior options</param>
        /// <param name="lastUpdatedBy">Last updater user name</param>
        /// <returns>Updated document</returns>
        public async Task<T> UpdateAsync(string id, T item, EventGridOptions options = null, string lastUpdatedBy = null)
        {
            /*
             Risk: 
                Since we don't validate namespace, user could update 
                parent document with child document by it's id. Vice versa.
                [no resolution yet]
             */
            var client = _client.Value;
            var container = client.GetContainer(this._databaseId, this._collectionId);
            var partitionKey = GeneratePartitionKey(item);

            var oldValue = await GetByIdAsync(id, partitionKey);

            if (oldValue == null) throw new ArgumentException("Error when update data, old data not found", "id");

            item.PartitionKey = oldValue?.PartitionKey;
            item.CreatedBy = oldValue?.CreatedBy;
            item.CreatedDate = oldValue.CreatedDate;

            if (!string.IsNullOrWhiteSpace(lastUpdatedBy)) item.LastUpdatedBy = lastUpdatedBy;
            item.LastUpdatedDate = DateTime.UtcNow;

            var updatedItem = await container.ReplaceItemAsync<T>(item, id, new PartitionKey(partitionKey));

            if (_cacheKeyPrefix != "")
            {
                await CacheManager.RemoveObject(_cacheKeyPrefix + id);
            }

            var eventGridSubject = "Update/";
            var shouldPublishEvent = true;

            if (options != null)
            {
                eventGridSubject = options.Subject;
                shouldPublishEvent = options.PublishEvent;
            }

            if (_eventGridEndpoint != null && shouldPublishEvent)
            {
                await new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    DataVersion = "1.0",
                    EventTime = DateTime.Now,
                    Subject = eventGridSubject,
                    EventType = typeof(T).ToString(),
                    Data = JsonConvert.SerializeObject(item)
                }.PublishAsync(_eventGridEndpoint, _eventGridKey);
            }

            return updatedItem;
        }

        /// <summary>
        /// Create or Update document
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="item">Document to be created</param>
        /// <param name="options">Eventgrid behavior options</param>
        /// <param name="manuallyManageIStandardWhoPropertyValue">Manually manage IStandardWho property. Note: using automatic management could slow-down insertion process</param>
        /// <param name="createdBy">Creator user name</param>
        /// <param name="lastUpdatedBy">Last updater user name</param>
        /// <param name="activeFlag">active Flag status on record creation</param>
        /// <returns>Newly created/updated document</returns>
        public async Task<T> UpsertAsync(string id, T item, EventGridOptions options = null,
            bool manuallyManageIStandardWhoPropertyValue = true, string createdBy = null,
            string lastUpdatedBy = null, string activeFlag = null)
        {
            /*
             Risk: 
                Since we don't validate namespace, user could update 
                parent document with child document by it's id. Vice versa.
                [no resolution yet]
             */
            var client = _client.Value;
            var container = client.GetContainer(this._databaseId, this._collectionId);

            var now = DateTime.UtcNow;

            if (!manuallyManageIStandardWhoPropertyValue)
            {
                var previousItems = await GetAsync(p => p.Id == id);

                if (previousItems.Items.Count() > 0)
                {
                    var previousItem = previousItems.Items.First();

                    item.CreatedBy = previousItem.CreatedBy;
                    item.CreatedDate = previousItem.CreatedDate;
                    if (!string.IsNullOrWhiteSpace(lastUpdatedBy)) item.LastUpdatedBy = lastUpdatedBy;
                    item.LastUpdatedDate = now;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(createdBy)) item.CreatedBy = createdBy;
                    item.CreatedDate = now;
                    if (!string.IsNullOrWhiteSpace(createdBy)) item.LastUpdatedBy = createdBy;
                    item.LastUpdatedDate = now;
                    item.ActiveFlag = activeFlag ?? "Y";
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(lastUpdatedBy)) item.LastUpdatedBy = lastUpdatedBy;
                item.LastUpdatedDate = now;
            }

            item.PartitionKey = GeneratePartitionKey(item);

            if (item.Id == null)
                item.Id = Guid.NewGuid().ToString();

            var upsertedItem = await container.UpsertItemAsync<T>(item);

            if (_cacheKeyPrefix != "")
            {
                await CacheManager.RemoveObject(_cacheKeyPrefix + id);
            }

            var eventGridSubject = "Upsert/";
            var shouldPublishEvent = true;

            if (options != null)
            {
                eventGridSubject = options.Subject;
                shouldPublishEvent = options.PublishEvent;
            }

            if (_eventGridEndpoint != null && shouldPublishEvent)
            {
                await new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    DataVersion = "1.0",
                    EventTime = DateTime.UtcNow,
                    Subject = eventGridSubject,
                    EventType = typeof(T).ToString(),
                    Data = JsonConvert.SerializeObject(item)
                }.PublishAsync(_eventGridEndpoint, _eventGridKey);
            }

            return upsertedItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Document id</param>
        /// <param name="partitionKeys">Partition keys</param>
        /// <param name="options">Event grid options</param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(string id,
            Dictionary<string, string> partitionKeys = null, EventGridOptions options = null)
        {
            var partitionKey = ComposePartitionKeys(partitionKeys);
            await DeleteAsync(id, partitionKey, options);
        }

        /// <summary>
        /// Delete document
        /// </summary>
        /// <param name="id">Document id</param>
        private async Task DeleteAsync(string id, string partitionKey, EventGridOptions options)
        {
            /*
             Risk: 
                Since we don't validate namespace, user could delete 
                any document in the collection by it's id, regardless it's namespace.
                [no resolution yet]
             */

            var client = _client.Value;
            var container = client.GetContainer(this._databaseId, this._collectionId);

            var deleteWithoutReturningDocumentDetail = false;

            if (options != null)
            {
                deleteWithoutReturningDocumentDetail = options.DeleteWithoutReturningDocumentDetail;
            }

            T removedItem = null;

            if (!deleteWithoutReturningDocumentDetail)
            {
                removedItem = await GetByIdAsync(id, partitionKey);

                if (removedItem == null)
                    return;
            }

            await container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));

            if (_cacheKeyPrefix != "")
            {
                await CacheManager.RemoveObject(_cacheKeyPrefix + id);
            }

            var eventGridSubject = "Delete/";
            var shouldPublishEvent = true;

            if (options != null)
            {
                eventGridSubject = options.Subject;
                shouldPublishEvent = options.PublishEvent;
            }

            if (_eventGridEndpoint != null && shouldPublishEvent)
            {
                var eventData = new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    DataVersion = "1.0",
                    EventTime = DateTime.Now,
                    Subject = eventGridSubject,
                    EventType = typeof(T).ToString()
                };

                if (deleteWithoutReturningDocumentDetail)
                {
                    eventData.Data = id;
                }
                else
                {
                    eventData.Data = removedItem;
                }

                await eventData.PublishAsync(_eventGridEndpoint, _eventGridKey);
            }

        }

        /// <summary>
        /// Delete all documents
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAllAsync()
        {
            var data = await GetAsync("select c.id from c");

            var items = data.Items.ToList();
            try
            {
                foreach (var item in items)
                {
                    //var partitionKey = new Dictionary<string, string>();
                    //partitionKey.Add("", item.partitionKey.ToString());
                    await DeleteAsync(item?.Id.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public async Task DeleteAllAsync(Dictionary<string, string> partitionKey = null)
        {
            var data = await GetAsync("select c.id, c.partitionKey from c");

            var items = data.Items.ToList();
            try
            {
                foreach (var item in items)
                {
                    if (partitionKey == null)
                    {
                        partitionKey = new Dictionary<string, string>();
                        partitionKey.Add("", item.PartitionKey.ToString());
                        await DeleteAsync(item.Id.ToString(), partitionKey);
                    }
                    else
                    {
                        await DeleteAsync(item.Id.ToString(), partitionKey);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Change Database Throughput
        /// </summary>
        /// <param name="throughput">New size</param>
        /// <returns></returns>
        public async Task ChangeThroughput(int throughput)
        {
            var client = _client.Value;
            var database = client.GetDatabase(_databaseId);

            await database.ReplaceThroughputAsync(throughput);

            throw new NotImplementedException();
        }
        #endregion

        #region "Private"

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                var client = _client.Value;
                await client.CreateDatabaseIfNotExistsAsync(_databaseId, throughput: 500);
            }
            catch (Exception e)
            {
                throw new Exception("Error when creating database", e);
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                var client = _client.Value;
                var database = client.GetDatabase(_databaseId);

                if (database == null)
                {
                    await CreateDatabaseIfNotExistsAsync();
                }

                await database.CreateContainerIfNotExistsAsync(new ContainerProperties(_collectionId, "/partitionKey"));
            }
            catch (Exception e)
            {
                throw new Exception("Error when creating container", e);
            }
        }

        private string ComposePartitionKeys(Dictionary<string, string> partitionKeys)
        {
            var partitionKey = _collectionId;

            if (_partitionPropertyDefined && partitionKey == null)
                throw new ArgumentException("Partition key must be defined");

            if (partitionKeys != null)
            {
                if (!_partitionPropertyDefined)
                    return partitionKey;
                for (int ii = 0; ii < _partitionPropertyNames.Count; ii++)
                {
                    try
                    {
                        partitionKey += "/" + partitionKeys[_partitionPropertyNames[ii]];
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException($"Missing partition key parameter {_partitionPropertyNames[ii]}");
                    }
                }
            }

            return partitionKey;
        }

        #endregion
    }
}