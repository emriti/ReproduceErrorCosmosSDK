using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nexus.Base.CosmosDBRepository
{
    /// <summary>
    /// Manage singleton connection handler to save overhead
    /// </summary>
    public static class ConnectionHandler
    {
        /// <summary>
        /// List of existing CosmosDB collection
        /// </summary>
        public static List<string> RegisteredCollections = new List<string>();

        /// <summary>
        /// CosmosDB client connection handler list
        /// </summary>
        public static ConcurrentDictionary<string, CosmosClient> DocumentHandler = new ConcurrentDictionary<string, CosmosClient>();

    }
}
