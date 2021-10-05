namespace Nexus.Base.CosmosDBRepository
{
    public class EventGridOptions
    {
        public bool PublishEvent { get; set; } = true;

        public string Subject { get; set; }

        public bool DeleteWithoutReturningDocumentDetail { get; set; } = false;
    }
}
