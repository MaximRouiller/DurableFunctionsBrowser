using DurableTask.AzureStorage;
using DurableTask.Core;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctionsBrowser.Infrastructure
{
    public class AzureStorageRepository
    {
        private readonly IOptions<AzureStorageConfiguration> configuration;

        public AzureStorageRepository(IOptions<AzureStorageConfiguration> configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IEnumerable<OrchestrationInstanceStatus>> GetAllDurableInstancesAsync()
        {
            CloudStorageAccount storageAccount;
            if (!CloudStorageAccount.TryParse(configuration.Value.ConnectionString, out storageAccount))
            {
                throw new ArgumentException("Could not parse Azure Storage Connection String.");
            }

            CloudTableClient client = storageAccount.CreateCloudTableClient();
            var table = client.GetTableReference($"{configuration.Value.ApplicationName}Instances");

            if (!table.Exists()) throw new ArgumentException($"Durable Functions Instances table does not exist.");

            TableQuery<OrchestrationInstanceStatus> query = new TableQuery<OrchestrationInstanceStatus>();
            TableQuerySegment<OrchestrationInstanceStatus> result = await table.ExecuteQuerySegmentedAsync<OrchestrationInstanceStatus>(query, new TableContinuationToken());
            return result.Results;
        }

        public async Task<IEnumerable<HistoryEvent>> GetAllEventsForInstanceIdAsync(string instanceId)
        {
            var client = new TaskHubClient(new AzureStorageOrchestrationService(new AzureStorageOrchestrationServiceSettings
            {
                TaskHubName = configuration.Value.ApplicationName,
                StorageConnectionString = configuration.Value.ConnectionString
            }));

            OrchestrationState orchestrationState = await client.GetOrchestrationStateAsync(instanceId);            
            string historyEvents = await client.GetOrchestrationHistoryAsync(orchestrationState.OrchestrationInstance);
            IList<HistoryEvent> history = JsonConvert.DeserializeObject<IList<HistoryEvent>>(historyEvents);
            return history.OrderBy(x => x.Timestamp).ToList();
        }

    }

    public class OrchestrationInstance
    {
        public string InstanceId { get; set; }
        public string ExecutionId { get; set; }
    }
    public class HistoryEvent : TableEntity
    {

        public bool IsPlayed { get; set; }

        public OrchestrationInstance OrchestrationInstance { get; set; }

        public DurableTask.Core.History.EventType EventType { get; set; }

        public string Input { get; set; }
        public string Name { get; set; }
        public string Result { get; set; }
        public string Reason { get; set; }
        public int? EventId { get; set; }
        public int? TaskScheduledId { get; set; }

    }

    public class OrchestrationInstanceStatus : TableEntity
    {
        public string ExecutionId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string CustomStatus { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string RuntimeStatus { get; set; }
    }


}
