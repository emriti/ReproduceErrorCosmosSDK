using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Nexus.Base.CosmosDBRepository;
using System.Collections.Generic;

namespace Nexus.Base.CosmosDBRepositoryTests.DAL
{
    public static class Repositories
    {
        public class ClassSessionContentRepository : DocumentDBRepository<ClassSessionContent>
        {
            public ClassSessionContentRepository(CosmosClient client) :
                base(databaseId: "Course", cosmosDBClient: client)
            {

            }
        }
    }
}
