using aspAPI.Models;
using aspAPI.Repositories;
using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var elasticUrl = builder.Configuration["ElasticsearchSettings:Url"] ?? "http://localhost:9200";

var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
    .DefaultMappingFor<Report>(m => m.IndexName("field-reports-index"));

var client = new ElasticsearchClient(settings);
builder.Services.AddSingleton(client);

builder.Services.AddControllers();
builder.Services.AddScoped<IReportRepo, ReportRepo>();

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
