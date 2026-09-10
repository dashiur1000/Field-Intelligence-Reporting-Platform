using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using System.Text.Json;
using concumer.Models;
using concumer.Validation;

namespace concumer.Data
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly ElasticsearchClient _elasticClient;
        private readonly string _indexName = "field-reports-index";

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, ElasticsearchClient elasticClient)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // מומלץ לוודא שהברירת מחדל היא kafka:9092 כדי למנוע חיפוש לוקאלי בתוך הקונטיינר
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "kafka:9092";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "field-reports-processor-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("field-reports");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult == null) continue;

                    string jsonMessage = consumeResult.Message.Value;
                    Report? report;
                    try
                    {
                        report = JsonSerializer.Deserialize<Report>(jsonMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"{ex.Message}");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    if (report == null)
                    {
                        _logger.LogInformation("The report is empty");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    string? rejectionReason = ValidateReportFunc.ValidateReport(report);
                    if (rejectionReason != null)
                    {
                        _logger.LogWarning($"Report rejected. ID: {report.reportId ?? "Unknown"}, Reason: {rejectionReason}");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    var existsResponse = await _elasticClient.ExistsAsync<Report>(report.reportId, cancellationToken: stoppingToken);
                    if (existsResponse.Exists)
                    {
                        _logger.LogInformation($"Duplicate report detected. ReportId: {report.reportId} already exists.");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    report.processedAt = DateTime.UtcNow;

                    var indexResponse = await _elasticClient.IndexAsync(report, idx => idx
                        .Index(_indexName)
                        .Id(report.reportId),
                        cancellationToken: stoppingToken
                    );

                    if (indexResponse.IsSuccess())
                    {
                        _logger.LogInformation($"Successfully saved report {report.reportId} to Elasticsearch.");
                    }
                    else
                    {
                        _logger.LogError($"Failed to save report {report.reportId} to Elasticsearch.");
                    }

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
            }
            consumer.Close();
        }
    }
}