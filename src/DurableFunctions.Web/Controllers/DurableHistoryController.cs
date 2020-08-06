using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableFunctionsBrowser.Infrastructure;
using HistoryEventType = DurableTask.Core.History.EventType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsBrowser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DurableHistoryController : ControllerBase
    {
        private AzureStorageRepository repository;

        public DurableHistoryController(AzureStorageRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<IEnumerable<HistoryEvent>> GetAllHistoryForInstanceAsync(string id)
        {

            return await repository.GetAllEventsForInstanceIdAsync(id);
        }
    }

    [Route("OrchestrationInstances/Instance")]
    [ApiController]
    public class EventVisualizationController : ControllerBase
    {
        private AzureStorageRepository repository;

        public EventVisualizationController(AzureStorageRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{id}.json")]
        public async Task<IEnumerable<Visualization>> GetAllEvents(string id)
        {
            IEnumerable<HistoryEvent> allEvents = await repository.GetAllEventsForInstanceIdAsync(id);
            var visualizationDataList = new List<VisualizationData>();

            var orchestratorDataPoints = new List<HistoryEvent>();
            string orchestratorName = allEvents.First(x => x.EventType == HistoryEventType.ExecutionStarted)?.Name;
            orchestratorDataPoints.AddRange(allEvents.Where(x => x.EventType == HistoryEventType.OrchestratorStarted || x.EventType == HistoryEventType.OrchestratorCompleted));

            var orchestratorVisualizationData = new VisualizationData
            {
                Label = orchestratorName,
                Data = orchestratorDataPoints.Select(x => new DataPoint
                {
                    TimeRange = new DateTimeOffset[] { x.Timestamp, x.Timestamp.AddSeconds(0.5) },
                    Value = x.EventType.ToString()
                })
            };
            visualizationDataList.Add(orchestratorVisualizationData);

            int maxEvents = allEvents.Max(x => x.EventId) ?? -1;
            if (maxEvents > -1)
            {

                for (int eventId = 0; eventId <= maxEvents; eventId++)
                {
                    var visualizationData = new VisualizationData
                    {
                        Label = $"{allEvents.First(x => x.EventId == eventId).Name}_{eventId}",
                        Data = allEvents.Where(x => x.EventId == eventId || x.TaskScheduledId == eventId)
                            .Select(x => new DataPoint
                            {
                                TimeRange = new DateTimeOffset[] { x.Timestamp, x.Timestamp.AddSeconds(0.5) },
                                Value = x.EventType.ToString()
                            }).ToList()
                    };
                    visualizationDataList.Add(visualizationData);
                }

            }


            return new List<Visualization>() { new Visualization {
                Group = id,
                Data = visualizationDataList
            } };
        }
    }

    public class Visualization
    {
        [JsonPropertyName("group")]
        public string Group { get; set; }

        [JsonPropertyName("data")]
        public IEnumerable<VisualizationData> Data { get; set; }
    }

    public class VisualizationData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("data")]
        public IEnumerable<DataPoint> Data { get; set; }
    }

    public class DataPoint
    {
        [JsonPropertyName("timeRange")]
        public DateTimeOffset[] TimeRange { get; set; }

        [JsonPropertyName("val")]
        public string Value { get; set; }
    }
}

// get execution events
// PartitionKey eq '{id}' and EventType eq 'ExecutionStarted' or EventType eq 'ExecutionCompleted'

// get all orchestrator events
// PartitionKey eq '{id}' and EventType eq 'OrchestratorStarted' or EventType eq 'OrchestratorCompleted'

// get all activities events
// PartitionKey eq '{id}' and TaskScheduledId gt -1 or EventId gt -1