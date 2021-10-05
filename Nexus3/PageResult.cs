using System.Collections.Generic;

namespace Nexus.Base.CosmosDBRepository
{
    public class PageResult<T>
    {
        public string ContinuationToken { get; private set; }
        public IEnumerable<T> Items { get; private set; }

        public double RequestCharge { get; private set; }

        public PageResult(IEnumerable<T> item, string continuationToken, double requestCharge = 0.0)
        {
            Items = item;
            ContinuationToken = continuationToken;
            RequestCharge = requestCharge;
        }
    }
}
