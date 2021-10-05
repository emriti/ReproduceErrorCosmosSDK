using Microsoft.Azure.EventGrid.Models;
using System;

namespace Nexus.Base.CosmosDBRepository
{
    public class SynchronizationException
    {
        public Exception Exception { get; set; }
        public EventGridEvent EventGridEvent { get; set; }
    }
}
