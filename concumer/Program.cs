using concumer.Data;
using concumer.Models;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;


var builder = WebApplication.CreateBuilder(args);

var elasticUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200";
var settings = new ElasticsearchClientSettings(new Uri("http://es:9200"))
    .DefaultMappingFor<Report>(m => m.IndexName("field-reports-index"));

var client = new ElasticsearchClient(settings);

string indexName = "field-reports-index";

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "field-reports-processor-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
};

var existsResponse = await client.Indices.ExistsAsync(indexName);
if (!existsResponse.Exists)
{
    var createResponse = await client.Indices.CreateAsync(indexName, c => c
        .Mappings(m => m
            .Properties<Report>(p => p
                .Keyword(t => t.reportId)
                .Date(t => t.timestamp)
                .Date(t => t.processedAt)
                .Keyword(t => t.agentId)
                .Keyword(t => t.unit)
                .Keyword(t => t.theater)
                .Keyword(t => t.sector)
                .Text(t => t.location)
                .Keyword(t => t.reportType)
                .Keyword(t => t.priority)
                .Keyword(t => t.sourceType)
                .Text(t => t.message)
                .Keyword(t => t.subjectId)
                .Keyword(t => t.subjectType)
            )
        )
    );
}


builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddSingleton(client);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();