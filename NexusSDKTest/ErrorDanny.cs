using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using static Nexus.Base.CosmosDBRepositoryTests.DAL.Repositories;

namespace Nexus.Base.CosmosDBRepositoryTests
{
    public class ErrorDanny
    {
        private static CosmosClient client;
        public ErrorDanny(CosmosClient _client)
        {
            client ??= _client;
        }

        [FunctionName("ErrorDannyPaging")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
        {
            try
            {
                var repsClassSessionContent = new ClassSessionContentRepository(client);
                var contentForumSessionIds = (await repsClassSessionContent.GetAsync(p =>
                    p.IsDeleted == false &&
                    p.ContentType == "Forum",
                    //classSessionIds.Contains(p.ClassSessionId),
                    usePaging: true,
                    pageSize: 12
                    //, selector: s => new ClassSessionContent { ClassSessionId = s.ClassSessionId }
                    )).Items.ToList();
                return new OkObjectResult(contentForumSessionIds);
            }
            catch (System.Exception e)
            {
                log.LogError($"Error : {e.Message}.");
                return new BadRequestObjectResult($"Error : {e.Message}."); ;
            }

        }
    }
}
